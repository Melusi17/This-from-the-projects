using IbhayiPharmacy.Data;
using IbhayiPharmacy.Models;
using IbhayiPharmacy.Models.PharmacistVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IbhayiPharmacy.Controllers
{
    [Authorize(Policy = "Customer")]
    public class CustomerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Main Dashboard View
        public IActionResult Index()
        {
            return View();
        }

        // Upload Prescription Section - GET
        public async Task<IActionResult> UploadPrescription()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var model = new CustomerDashboardVM
            {
                UnprocessedPrescriptions = await _context.Prescriptions
                    .Where(p => p.ApplicationUserId == userId &&
                           (p.Status == null || p.Status == "Unprocessed" || p.Status == "Pending"))
                    .OrderByDescending(p => p.DateIssued)
                    .ToListAsync(),

                ProcessedPrescriptions = await _context.Prescriptions
                    .Where(p => p.ApplicationUserId == userId &&
                           (p.Status == "Processed" || p.Status == "Partially Processed"))
                    .Include(p => p.Doctors)
                    .OrderByDescending(p => p.DateIssued)
                    .ToListAsync()
            };

            return View(model);
        }

        // Upload Prescription - POST (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UploadPrescription(IFormFile file, bool dispenseUponApproval)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "Please select a file to upload." });
                }

                // Validate file type
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (fileExtension != ".pdf")
                {
                    return Json(new { success = false, message = "Only PDF files are allowed." });
                }

                // Validate file size (optional: limit to 10MB)
                if (file.Length > 10 * 1024 * 1024) // 10MB
                {
                    return Json(new { success = false, message = "File size must be less than 10MB." });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Create new prescription
                var prescription = new Prescription
                {
                    ApplicationUserId = userId,
                    DateIssued = DateTime.Now,
                    DispenseUponApproval = dispenseUponApproval,
                    Status = "Unprocessed"
                };

                // Store file as byte array in database ONLY (no file system storage)
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    prescription.Script = ms.ToArray();
                }

                // Save to database
                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Prescription uploaded successfully!",
                    prescriptionId = prescription.PrescriptionID,
                    date = prescription.DateIssued.ToString("yyyy-MM-dd"),
                    status = prescription.Status,
                    dispense = prescription.DispenseUponApproval ? "Yes" : "No"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error uploading prescription: {ex.Message}" });
            }
        }

        // Download Prescription
        public async Task<IActionResult> DownloadPrescription(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var prescription = await _context.Prescriptions
                .FirstOrDefaultAsync(p => p.PrescriptionID == id && p.ApplicationUserId == userId);

            if (prescription == null || prescription.Script == null)
                return NotFound();

            return File(prescription.Script, "application/pdf", $"Prescription_{id.ToString("D3")}.pdf");
        }

        // Place Orders Section
        public async Task<IActionResult> PlaceOrder()
        {
            var model = new PlaceOrderVM
            {
                Doctors = await _context.Doctors.ToListAsync(),
                Medications = await _context.Medications
                    .Include(m => m.DosageForm)
                    .Include(m => m.Medication_Ingredients)
                    .ThenInclude(mi => mi.Active_Ingredients)
                    .ToListAsync(),
                OrderDate = DateTime.Now
            };

            return View(model);
        }

        // Track Orders Section - SIMPLIFIED (Server-Side Rendering)
        public async Task<IActionResult> TrackOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders
                .Where(o => o.Customer.ApplicationUserId == userId && o.Status == "Ordered")
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Medications)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.ScriptLine)
                        .ThenInclude(sl => sl.Prescriptions)
                            .ThenInclude(p => p.Doctors)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders); // Pass orders directly to the view
        }

        // Manage Repeats Section
        public async Task<IActionResult> ManageRepeats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var repeats = await _context.ScriptLines
                .Where(sl => sl.Prescriptions.ApplicationUserId == userId &&
                       sl.RepeatsLeft > 0)
                .Include(sl => sl.Medications)
                .Include(sl => sl.Prescriptions)
                .Select(sl => new RepeatMedicationVM
                {
                    ScriptLineId = sl.ScriptLineID,
                    MedicationName = sl.Medications.MedicationName,
                    RepeatsLeft = sl.RepeatsLeft,
                    LastRefillDate = sl.ApprovedDate ?? DateTime.Now.AddDays(-30),
                    Instructions = sl.Instructions
                })
                .ToListAsync();

            return View(repeats);
        }

        // View Reports Section
        public IActionResult ViewReports()
        {
            return View();
        }

        // API: Get medications for a specific prescription
        [HttpGet]
        public async Task<JsonResult> GetPrescriptionMedications(int prescriptionId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var prescription = await _context.Prescriptions
                    .Include(p => p.scriptLines!)
                    .ThenInclude(sl => sl.Medications)
                    .FirstOrDefaultAsync(p => p.PrescriptionID == prescriptionId && p.ApplicationUserId == userId);

                if (prescription == null || prescription.scriptLines == null)
                    return Json(new List<object>());

                var medications = prescription.scriptLines
                    .Select(sl => new
                    {
                        scriptLineId = sl.ScriptLineID,
                        medicationId = sl.MedicationID,
                        name = sl.Medications?.MedicationName ?? "Unknown Medication",
                        instructions = sl.Instructions ?? "Take as directed",
                        repeats = sl.Repeats,
                        price = sl.Medications?.CurrentPrice ?? 0,
                        status = sl.Status ?? "Pending",
                        rejectionReason = sl.RejectionReason ?? "",
                        quantity = sl.Quantity
                    })
                    .ToList();

                return Json(medications);
            }
            catch (Exception)
            {
                return Json(new List<object>());
            }
        }

        // API: Get medication details
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
            catch (Exception)
            {
                return Json(new { error = "Error loading medication details" });
            }
        }

        // API: Check for allergy conflicts
        [HttpGet]
        public async Task<JsonResult> CheckAllergyConflicts(int medicationId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

                if (customer == null)
                    return Json(new { hasConflicts = false, conflicts = new string[0] });

                var customerAllergies = await _context.Custormer_Allergies
                    .Where(ca => ca.CustomerID == customer.CustormerID)
                    .Include(ca => ca.Active_Ingredient)
                    .Select(ca => ca.Active_Ingredient!.Name)
                    .ToListAsync();

                var medicationIngredients = await _context.Medication_Ingredients
                    .Where(mi => mi.MedicationID == medicationId)
                    .Include(mi => mi.Active_Ingredients)
                    .Select(mi => mi.Active_Ingredients!.Name)
                    .ToListAsync();

                var conflicts = customerAllergies
                    .Where(allergy => medicationIngredients.Any(ingredient =>
                        ingredient.Contains(allergy, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                return Json(new { hasConflicts = conflicts.Any(), conflicts });
            }
            catch (Exception)
            {
                return Json(new { hasConflicts = false, conflicts = new string[0] });
            }
        }

        // API: Submit order - UPDATED
        [HttpPost]
        public async Task<JsonResult> SubmitOrder([FromBody] OrderSubmissionVM orderData)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

                if (customer == null)
                    return Json(new { success = false, message = "Customer not found" });

                // Create new order - UPDATED STATUS
                var order = new Order
                {
                    CustomerID = customer.CustormerID,
                    OrderDate = DateTime.Now,
                    Status = "Ordered", // CHANGED FROM "Pending" TO "Ordered"
                    VAT = 15 // 15% VAT
                };

                // Calculate total
                decimal subtotal = 0;
                foreach (var item in orderData.OrderItems)
                {
                    var medication = await _context.Medications
                        .FirstOrDefaultAsync(m => m.MedcationID == item.MedicationId);

                    if (medication != null)
                    {
                        subtotal += medication.CurrentPrice * item.Quantity;

                        var orderLine = new OrderLine
                        {
                            MedicationID = item.MedicationId,
                            Quantity = item.Quantity,
                            ItemPrice = medication.CurrentPrice,
                            ScriptLineID = item.ScriptLineId,
                            Status = "Pending" // NEW: Set default status for order line
                        };

                        order.OrderLines.Add(orderLine);
                    }
                }

                order.TotalDue = (subtotal + (subtotal * order.VAT / 100)).ToString("F2");

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Order submitted successfully!", orderId = order.OrderID });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error submitting order: {ex.Message}" });
            }
        }

        // API: Request refill
        [HttpPost]
        public async Task<JsonResult> RequestRefill(int scriptLineId)
        {
            try
            {
                var scriptLine = await _context.ScriptLines
                    .FirstOrDefaultAsync(sl => sl.ScriptLineID == scriptLineId);

                if (scriptLine == null)
                    return Json(new { success = false, message = "Prescription line not found" });

                if (scriptLine.RepeatsLeft <= 0)
                    return Json(new { success = false, message = "No repeats left for this medication" });

                scriptLine.RepeatsLeft--;
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Refill requested successfully!",
                    repeatsLeft = scriptLine.RepeatsLeft
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error requesting refill: {ex.Message}" });
            }
        }

        // API: Generate PDF Report
        [HttpPost]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequestVM request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var reportData = await _context.Orders
                    .Where(o => o.Customer.ApplicationUserId == userId &&
                           o.OrderDate.Date == request.ReportDate.Date)
                    .Include(o => o.OrderLines)
                        .ThenInclude(ol => ol.Medications)
                    .Select(o => new
                    {
                        OrderDate = o.OrderDate,
                        Medications = o.OrderLines.Select(ol => new
                        {
                            Name = ol.Medications.MedicationName,
                            Quantity = ol.Quantity,
                            Price = ol.ItemPrice,
                            Status = ol.Status // NEW: Include status in report
                        }),
                        Total = o.TotalDue
                    })
                    .ToListAsync();

                return Json(new { success = true, data = reportData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating report: {ex.Message}" });
            }
        }

        // Profile Management
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.ApplicationUsers
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ApplicationUser model)
        {
            if (!ModelState.IsValid)
                return View("Profile", model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.ApplicationUsers
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            user.Name = model.Name;
            user.Surname = model.Surname;
            user.Email = model.Email;
            user.CellphoneNumber = model.CellphoneNumber;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
    }
}