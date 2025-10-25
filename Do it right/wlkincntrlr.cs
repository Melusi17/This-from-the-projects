using IbhayiPharmacy.Data;
using IbhayiPharmacy.Models;
using IbhayiPharmacy.Models.PharmacistVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IbhayiPharmacy.Controllers
{
    [Authorize(Policy = "Pharmacist")]
    public class WalkInPrescriptionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly Random _random = new Random();

        public WalkInPrescriptionController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Main walk-in prescription creation form
        public async Task<IActionResult> Create()
        {
            try
            {
                var viewModel = new WalkInPrescriptionVM
                {
                    PrescriptionDate = DateTime.Now,
                    ScriptLines = new List<WalkInScriptLineVM>
                    {
                        new WalkInScriptLineVM() // Start with one empty medication row
                    }
                };

                await LoadViewData();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading form: {ex.Message}";
                return View(new WalkInPrescriptionVM());
            }
        }

        // POST: Save walk-in prescription and create order (FIXED VERSION)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WalkInPrescriptionVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadViewData();
                    return View(model);
                }

                // Validate required data
                if (!model.CustomerId.HasValue)
                {
                    ModelState.AddModelError("", "Please select or register a patient.");
                    await LoadViewData();
                    return View(model);
                }

                if (!model.DoctorId.HasValue)
                {
                    ModelState.AddModelError("", "Please select a doctor.");
                    await LoadViewData();
                    return View(model);
                }

                var validMedications = model.ScriptLines?.Where(sl => sl.MedicationId > 0).ToList();
                if (validMedications == null || !validMedications.Any())
                {
                    ModelState.AddModelError("", "Please add at least one medication.");
                    await LoadViewData();
                    return View(model);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 1. Create Prescription with "WalkIn" status
                    var prescription = new Prescription
                    {
                        ApplicationUserId = await GetCustomerUserId(model.CustomerId.Value),
                        DoctorID = model.DoctorId.Value,
                        DateIssued = model.PrescriptionDate,
                        DispenseUponApproval = model.DispenseUponApproval,
                        Status = "WalkIn"
                    };

                    // Store PDF if uploaded
                    if (model.PrescriptionFile != null && model.PrescriptionFile.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await model.PrescriptionFile.CopyToAsync(ms);
                        prescription.Script = ms.ToArray();
                    }

                    _context.Prescriptions.Add(prescription);
                    await _context.SaveChangesAsync(); // Get PrescriptionID

                    // 2. Create ScriptLines (all automatically approved for walk-ins)
                    foreach (var scriptLineVM in validMedications)
                    {
                        var scriptLine = new ScriptLine
                        {
                            PrescriptionID = prescription.PrescriptionID,
                            MedicationID = scriptLineVM.MedicationId,
                            Quantity = scriptLineVM.Quantity,
                            Instructions = scriptLineVM.Instructions,
                            Repeats = scriptLineVM.IsRepeat ? 3 : 0, // Default 3 repeats if enabled
                            RepeatsLeft = scriptLineVM.RepeatsLeft,
                            Status = "Approved", // Auto-approved for walk-ins
                            ApprovedDate = DateTime.Now
                        };

                        _context.ScriptLines.Add(scriptLine);
                    }
                    await _context.SaveChangesAsync();

                    // 3. Create Order with "Ordered" status and "Pending" order lines
                    var order = await CreateOrderFromPrescription(prescription.PrescriptionID, model.CustomerId.Value);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = $"Walk-in prescription created successfully! Order {order.OrderNumber} is ready for dispensing.";
                    return RedirectToAction("Index", "PharmacistDispensing"); // Redirect to dispensing dashboard
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = $"Error creating walk-in prescription: {ex.Message}";
                    await LoadViewData();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                await LoadViewData();
                return View(model);
            }
        }

        private async Task<Order> CreateOrderFromPrescription(int prescriptionId, int customerId)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.scriptLines)
                    .ThenInclude(sl => sl.Medications)
                .FirstOrDefaultAsync(p => p.PrescriptionID == prescriptionId);

            if (prescription == null) throw new Exception("Prescription not found");

            // Generate order number
            string orderNumber = await GenerateUniqueOrderNumber();

            var order = new Order
            {
                CustomerID = customerId,
                OrderDate = DateTime.Now,
                Status = "Ordered", // Order status is "Ordered"
                VAT = 15,
                OrderNumber = orderNumber
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Get OrderID

            decimal subtotal = 0;

            // Create order lines with "Pending" status
            foreach (var scriptLine in prescription.scriptLines.Where(sl => sl.Status == "Approved"))
            {
                var orderLine = new OrderLine
                {
                    OrderID = order.OrderID,
                    ScriptLineID = scriptLine.ScriptLineID,
                    MedicationID = scriptLine.MedicationID,
                    Quantity = scriptLine.Quantity,
                    ItemPrice = (int)scriptLine.Medications.CurrentPrice,
                    Status = "Pending" // OrderLine status is "Pending"
                };

                _context.OrderLines.Add(orderLine);
                subtotal += scriptLine.Medications.CurrentPrice * scriptLine.Quantity;

                // DO NOT update inventory here - wait for dispensing in PharmacistDispensingController
            }

            order.TotalDue = (subtotal + (subtotal * order.VAT / 100)).ToString("F2");
            return order;
        }

        // AJAX: Search customers
        [HttpGet]
        public async Task<JsonResult> SearchCustomers(string searchTerm)
        {
            try
            {
                var query = _context.Customers
                    .Include(c => c.ApplicationUser)
                    .AsQueryable();

                // Only apply filter if searchTerm is not empty
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(c => c.ApplicationUser.Name.Contains(searchTerm) ||
                                           c.ApplicationUser.Surname.Contains(searchTerm) ||
                                           c.ApplicationUser.IDNumber.Contains(searchTerm));
                }

                var customers = await query
                    .Select(c => new
                    {
                        id = c.CustormerID,
                        text = $"{c.ApplicationUser.Name} {c.ApplicationUser.Surname} (ID: {c.ApplicationUser.IDNumber})",
                        name = c.ApplicationUser.Name,
                        surname = c.ApplicationUser.Surname,
                        idNumber = c.ApplicationUser.IDNumber
                    })
                    .Take(20) // Increased from 10 to 20 for better browsing
                    .ToListAsync();

                return Json(customers);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // AJAX: Search doctors  
        [HttpGet]
        public async Task<JsonResult> SearchDoctors(string searchTerm)
        {
            try
            {
                var query = _context.Doctors.AsQueryable();

                // Only apply filter if searchTerm is not empty
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(d => d.Name.Contains(searchTerm) ||
                                           d.Surname.Contains(searchTerm) ||
                                           d.HealthCouncilRegistrationNumber.Contains(searchTerm));
                }

                var doctors = await query
                    .Select(d => new
                    {
                        id = d.DoctorID,
                        text = $"Dr. {d.Name} {d.Surname} ({d.HealthCouncilRegistrationNumber})",
                        name = d.Name,
                        surname = d.Surname,
                        practiceNumber = d.HealthCouncilRegistrationNumber
                    })
                    .Take(20) // Increased from 10 to 20 for better browsing
                    .ToListAsync();

                return Json(doctors);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }


        // AJAX: Search medications
        [HttpGet]
        public async Task<JsonResult> SearchMedications(string searchTerm)
        {
            try
            {
                var query = _context.Medications
                    .Include(m => m.DosageForm)
                    .Include(m => m.Medication_Ingredients)
                        .ThenInclude(mi => mi.Active_Ingredients)
                    .AsQueryable();

                // Only apply filter if searchTerm is not empty
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(m => m.MedicationName.Contains(searchTerm));
                }

                var medications = await query
                    .Select(m => new
                    {
                        id = m.MedcationID,
                        name = m.MedicationName,
                        dosageForm = m.DosageForm.DosageFormName,
                        displayName = $"{m.MedicationName} ({m.DosageForm.DosageFormName})", // Combined name
                        schedule = m.Schedule,
                        price = m.CurrentPrice,
                        stock = m.QuantityOnHand,
                        reorderLevel = m.ReOrderLevel,
                        activeIngredients = m.Medication_Ingredients.Select(mi =>
                            $"{mi.Active_Ingredients.Name} {mi.Strength}").ToList()
                    })
                    .Take(20) // Increased from 10 to 20 for better browsing
                    .ToListAsync();

                return Json(medications);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // AJAX: Get allergy options from Active_Ingredients table
        [HttpGet]
        public async Task<JsonResult> GetAllergyOptions()
        {
            try
            {
                var allergies = await _context.Active_Ingredients
                    .Select(a => new
                    {
                        id = a.Active_IngredientID,
                        name = a.Name
                    })
                    .ToListAsync();

                return Json(allergies);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // AJAX: Get customer allergies from Customer_Allergies table
        [HttpGet]
        public async Task<JsonResult> GetCustomerAllergies(int customerId)
        {
            try
            {
                var allergies = await _context.Custormer_Allergies
                    .Where(ca => ca.CustomerID == customerId)
                    .Include(ca => ca.Active_Ingredient)
                    .Select(ca => new
                    {
                        id = ca.Active_Ingredient.Active_IngredientID,
                        name = ca.Active_Ingredient.Name
                    })
                    .ToListAsync();

                return Json(allergies);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // AJAX: Register new patient with real allergy system
        [HttpPost]
        public async Task<JsonResult> RegisterNewPatient([FromBody] RegisterPatientRequest request)
        {
            try
            {
                // Create ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    Name = request.Name,
                    Surname = request.Surname,
                    IDNumber = request.IDNumber,
                    CellphoneNumber = request.Cellphone
                };

                // Create user with password
                var passwordHasher = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

                _context.ApplicationUsers.Add(user);
                await _context.SaveChangesAsync();

                // Assign to Customer role
                if (!await _roleManager.RoleExistsAsync("Customer"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Customer"));
                }
                await _userManager.AddToRoleAsync(user, "Customer");

                // Create Customer record
                var customer = new Customer
                {
                    ApplicationUserId = user.Id
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync(); // Get CustomerID

                // Add allergies to Customer_Allergies table
                foreach (var allergyId in request.SelectedAllergyIds)
                {
                    var customerAllergy = new Custormer_Allergy
                    {
                        CustomerID = customer.CustormerID,
                        Active_IngredientID = allergyId
                    };
                    _context.Custormer_Allergies.Add(customerAllergy);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    customerId = customer.CustormerID,
                    customerName = $"{user.Name} {user.Surname}",
                    customerIDNumber = user.IDNumber,
                    message = "Patient registered successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // AJAX: Register new doctor
        [HttpPost]
        public async Task<JsonResult> RegisterNewDoctor([FromBody] RegisterDoctorRequest request)
        {
            try
            {
                var doctor = new Doctor
                {
                    Name = request.Name,
                    Surname = request.Surname,
                    HealthCouncilRegistrationNumber = request.PracticeNumber,
                    ContactNumber = request.ContactNumber,
                    Email = request.Email
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    doctorId = doctor.DoctorID,
                    doctorName = $"Dr. {doctor.Name} {doctor.Surname}",
                    practiceNumber = doctor.HealthCouncilRegistrationNumber,
                    message = "Doctor registered successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // AJAX: Check allergy conflicts against medication ingredients
        [HttpGet]
        public async Task<JsonResult> CheckAllergyConflicts(int customerId, int medicationId)
        {
            try
            {
                // Get customer allergies
                var customerAllergies = await _context.Custormer_Allergies
                    .Where(ca => ca.CustomerID == customerId)
                    .Include(ca => ca.Active_Ingredient)
                    .Select(ca => ca.Active_Ingredient.Name)
                    .ToListAsync();

                // Get medication ingredients
                var medicationIngredients = await _context.Medication_Ingredients
                    .Where(mi => mi.MedicationID == medicationId)
                    .Include(mi => mi.Active_Ingredients)
                    .Select(mi => mi.Active_Ingredients.Name)
                    .ToListAsync();

                // Find conflicts
                var conflicts = customerAllergies
                    .Where(allergy => medicationIngredients.Any(ingredient =>
                        ingredient.Contains(allergy, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                return Json(new
                {
                    hasConflicts = conflicts.Any(),
                    conflicts = conflicts
                });
            }
            catch (Exception ex)
            {
                return Json(new { hasConflicts = false, conflicts = new string[0] });
            }
        }

        // AJAX: Get medication details
        [HttpGet]
        public async Task<JsonResult> GetMedicationDetails(int medicationId)
        {
            try
            {
                var medication = await _context.Medications
                    .Include(m => m.Medication_Ingredients)
                        .ThenInclude(mi => mi.Active_Ingredients)
                    .Include(m => m.DosageForm)
                    .FirstOrDefaultAsync(m => m.MedcationID == medicationId);

                if (medication == null)
                    return Json(new { error = "Medication not found" });

                var activeIngredients = medication.Medication_Ingredients?
                    .Select(mi => $"{mi.Active_Ingredients?.Name} {mi.Strength}")
                    .ToList() ?? new List<string>();

                var isLowStock = medication.QuantityOnHand <= medication.ReOrderLevel + 10;

                return Json(new
                {
                    medicationName = medication.MedicationName,
                    activeIngredients = string.Join(", ", activeIngredients),
                    stock = medication.QuantityOnHand,
                    reorderLevel = medication.ReOrderLevel,
                    dosageForm = medication.DosageForm?.DosageFormName ?? "Unknown",
                    schedule = medication.Schedule,
                    price = medication.CurrentPrice,
                    isLowStock = isLowStock
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Error loading medication: {ex.Message}" });
            }
        }

        private async Task<string> GetCustomerUserId(int customerId)
        {
            var customer = await _context.Customers
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.CustormerID == customerId);

            return customer?.ApplicationUserId ?? throw new Exception("Customer not found");
        }

        private async Task<string> GenerateUniqueOrderNumber()
        {
            string orderNumber;
            bool isUnique;
            int maxAttempts = 5;
            int attempts = 0;

            do
            {
                // Format: ORD-W-RRRR (RRRR = 4 random digits)
                string randomPart = _random.Next(1000, 10000).ToString();
                orderNumber = $"ORD-W-{randomPart}";

                isUnique = !await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber);
                attempts++;

            } while (!isUnique && attempts < maxAttempts);

            // Fallback if we still don't have a unique number
            if (!isUnique)
            {
                string timestamp = DateTime.Now.ToString("HHmmss");
                orderNumber = $"ORD-W-{timestamp}";
            }

            return orderNumber;
        }

        private async Task LoadViewData()
        {
            // Preload any needed data for dropdowns
            ViewBag.DosageForms = await _context.DosageForms.ToListAsync();
        }
    }
}