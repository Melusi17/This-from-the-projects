using IbhayiPharmacy.Data;
using IbhayiPharmacy.Models;
using IbhayiPharmacy.Models.PharmacistVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IbhayiPharmacy.Controllers
{
    [Authorize(Policy = "Pharmacist")]
    public class ScriptsProcessedController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random = new Random();

        public ScriptsProcessedController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Index - Show unprocessed prescriptions
        public async Task<IActionResult> Index()
        {
            try
            {
                var unprocessedPrescriptions = await _context.Prescriptions
                    .Include(p => p.ApplicationUser)
                    .Include(p => p.Doctors)
                    .Where(p => string.IsNullOrEmpty(p.Status) || p.Status == "Unprocessed" || p.Status == "Pending")
                    .OrderByDescending(p => p.DateIssued)
                    .ToListAsync();

                return View(unprocessedPrescriptions);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading prescriptions: {ex.Message}";
                return View(new List<Prescription>());
            }
        }

        // GET: Edit - Show form to process prescription (regular processing)
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var prescription = await _context.Prescriptions
                    .Include(p => p.ApplicationUser)
                    .Include(p => p.Doctors)
                    .Include(p => p.scriptLines)
                    .ThenInclude(sl => sl.Medications)
                    .FirstOrDefaultAsync(p => p.PrescriptionID == id);

                if (prescription == null)
                {
                    TempData["ErrorMessage"] = "Prescription not found";
                    return RedirectToAction(nameof(Index));
                }

                var customer = await _context.Customers
                    .Include(c => c.ApplicationUser)
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == prescription.ApplicationUserId);

                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Customer not found for this prescription";
                    return RedirectToAction(nameof(Index));
                }

                var customerAllergies = await _context.Custormer_Allergies
                    .Where(ca => ca.CustomerID == customer.CustormerID)
                    .Include(ca => ca.Active_Ingredient)
                    .Select(ca => ca.Active_Ingredient.Name)
                    .ToListAsync();

                var viewModel = new CustomerScriptsVM
                {
                    Prescr = prescription.PrescriptionID,
                    Name = customer.ApplicationUser?.Name ?? "",
                    Surname = customer.ApplicationUser?.Surname ?? "",
                    IDNumber = customer.ApplicationUser?.IDNumber ?? "",
                    PrescriptionDate = prescription.DateIssued,
                    CustomerAllergies = customerAllergies,
                    ScriptList = new List<Prescription> { prescription },
                    DoctorId = prescription.DoctorID,
                    DoctorName = prescription.Doctors != null ?
                        $"{prescription.Doctors.Name} {prescription.Doctors.Surname}" : "",
                    ScriptLines = prescription.scriptLines.Select(sl => new ScriptLineVM
                    {
                        ScriptLineId = sl.ScriptLineID,
                        MedicationId = sl.MedicationID,
                        MedicationName = sl.Medications?.MedicationName ?? "",
                        Quantity = sl.Quantity,
                        Instructions = sl.Instructions ?? "",
                        IsRepeat = sl.Repeats > 0,
                        RepeatsLeft = sl.RepeatsLeft,
                        Status = sl.Status ?? "Pending",
                        RejectionReason = sl.RejectionReason,
                        CanBeApproved = prescription.DoctorID.HasValue
                    }).ToList()
                };

                if (!viewModel.ScriptLines.Any())
                {
                    viewModel.ScriptLines.Add(new ScriptLineVM
                    {
                        ScriptLineId = 0,
                        Status = "Pending",
                        Quantity = 1,
                        RepeatsLeft = 0,
                        IsRepeat = false,
                        Instructions = "",
                        CanBeApproved = prescription.DoctorID.HasValue
                    });
                }

                await ReloadViewBags();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading prescription: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: ProcessAndDispense - Show form to process prescription with automatic dispensing
        public async Task<IActionResult> ProcessAndDispense(int id)
        {
            try
            {
                var prescription = await _context.Prescriptions
                    .Include(p => p.ApplicationUser)
                    .Include(p => p.Doctors)
                    .Include(p => p.scriptLines)
                    .ThenInclude(sl => sl.Medications)
                    .FirstOrDefaultAsync(p => p.PrescriptionID == id);

                if (prescription == null)
                {
                    TempData["ErrorMessage"] = "Prescription not found";
                    return RedirectToAction(nameof(Index));
                }

                var customer = await _context.Customers
                    .Include(c => c.ApplicationUser)
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == prescription.ApplicationUserId);

                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Customer not found for this prescription";
                    return RedirectToAction(nameof(Index));
                }

                var customerAllergies = await _context.Custormer_Allergies
                    .Where(ca => ca.CustomerID == customer.CustormerID)
                    .Include(ca => ca.Active_Ingredient)
                    .Select(ca => ca.Active_Ingredient.Name)
                    .ToListAsync();

                var viewModel = new CustomerScriptsVM
                {
                    Prescr = prescription.PrescriptionID,
                    Name = customer.ApplicationUser?.Name ?? "",
                    Surname = customer.ApplicationUser?.Surname ?? "",
                    IDNumber = customer.ApplicationUser?.IDNumber ?? "",
                    PrescriptionDate = prescription.DateIssued,
                    CustomerAllergies = customerAllergies,
                    ScriptList = new List<Prescription> { prescription },
                    DoctorId = prescription.DoctorID,
                    DoctorName = prescription.Doctors != null ?
                        $"{prescription.Doctors.Name} {prescription.Doctors.Surname}" : "",
                    ScriptLines = prescription.scriptLines.Select(sl => new ScriptLineVM
                    {
                        ScriptLineId = sl.ScriptLineID,
                        MedicationId = sl.MedicationID,
                        MedicationName = sl.Medications?.MedicationName ?? "",
                        Quantity = sl.Quantity,
                        Instructions = sl.Instructions ?? "",
                        IsRepeat = sl.Repeats > 0,
                        RepeatsLeft = sl.RepeatsLeft,
                        Status = sl.Status ?? "Pending",
                        RejectionReason = sl.RejectionReason,
                        CanBeApproved = prescription.DoctorID.HasValue
                    }).ToList()
                };

                if (!viewModel.ScriptLines.Any())
                {
                    viewModel.ScriptLines.Add(new ScriptLineVM
                    {
                        ScriptLineId = 0,
                        Status = "Pending",
                        Quantity = 1,
                        RepeatsLeft = 0,
                        IsRepeat = false,
                        Instructions = "",
                        CanBeApproved = prescription.DoctorID.HasValue
                    });
                }

                await ReloadViewBags();
                return View("ProcessAndDispense", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading prescription: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Edit - Save to database (regular processing)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerScriptsVM model)
        {
            return await ProcessPrescriptionInternal(model, false);
        }

        // POST: ProcessAndDispense - Save to database and create order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessAndDispense(CustomerScriptsVM model)
        {
            // DEBUG: Log incoming data
            Console.WriteLine($"=== DEBUG: ProcessAndDispense STARTED ===");
            Console.WriteLine($"PrescriptionID: {model.Prescr}");
            Console.WriteLine($"DoctorId from model: {model.DoctorId}");
            Console.WriteLine($"ScriptLines count: {model.ScriptLines?.Count}");

            // TEMPORARY: Manual DoctorId extraction if model binding fails
            if (!model.DoctorId.HasValue || model.DoctorId == 0)
            {
                var doctorIdValue = Request.Form["DoctorId"].FirstOrDefault();
                if (!string.IsNullOrEmpty(doctorIdValue) && int.TryParse(doctorIdValue, out int doctorId))
                {
                    model.DoctorId = doctorId;
                    Console.WriteLine($"DEBUG: Extracted DoctorId from form: {model.DoctorId}");
                }
            }

            return await ProcessPrescriptionInternal(model, true);
        }

        // Internal method to handle both regular processing and process+dispense
        private async Task<IActionResult> ProcessPrescriptionInternal(CustomerScriptsVM model, bool createOrder)
        {
            try
            {
                Console.WriteLine($"=== DEBUG: ProcessPrescriptionInternal STARTED ===");
                Console.WriteLine($"CreateOrder: {createOrder}, PrescriptionID: {model.Prescr}");
                Console.WriteLine($"Model DoctorId: {model.DoctorId}");
                Console.WriteLine($"Model ScriptLines count: {model.ScriptLines?.Count}");

                // DEBUG: Log all received script lines
                if (model.ScriptLines != null)
                {
                    foreach (var sl in model.ScriptLines)
                    {
                        Console.WriteLine($"DEBUG RECEIVED - ScriptLineId: {sl.ScriptLineId}, MedicationId: {sl.MedicationId}, Status: {sl.Status}, Quantity: {sl.Quantity}");
                    }
                }

                // TEMPORARY: Skip ModelState validation for debugging
                // if (!ModelState.IsValid)
                // {
                //     Console.WriteLine($"DEBUG: ModelState invalid - {ModelState.ErrorCount} errors");
                //     foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                //     {
                //         Console.WriteLine($"DEBUG: ModelError: {error.ErrorMessage}");
                //     }
                //     TempData["ErrorMessage"] = "Please fix the validation errors below.";
                //     await ReloadViewBags();
                //     return View(createOrder ? "ProcessAndDispense" : "Edit", model);
                // }

                var prescription = await _context.Prescriptions
                    .Include(p => p.scriptLines)
                    .FirstOrDefaultAsync(p => p.PrescriptionID == model.Prescr);

                if (prescription == null)
                {
                    Console.WriteLine($"DEBUG: Prescription {model.Prescr} not found");
                    TempData["ErrorMessage"] = "Prescription not found";
                    return RedirectToAction(nameof(Index));
                }

                // DEBUG: Check what script lines we have
                Console.WriteLine($"DEBUG: Existing prescription script lines: {prescription.scriptLines?.Count}");

                var hasMedications = model.ScriptLines?.Any(sl => sl.MedicationId > 0) == true;
                Console.WriteLine($"DEBUG: HasMedications: {hasMedications}, ScriptLines count: {model.ScriptLines?.Count}");

                if (!hasMedications)
                {
                    Console.WriteLine("DEBUG: No medications found");
                    ModelState.AddModelError("", "Please add at least one medication to process the prescription.");
                    TempData["ErrorMessage"] = "Please add at least one medication to process the prescription.";
                    await ReloadViewBags();
                    return View(createOrder ? "ProcessAndDispense" : "Edit", model);
                }

                var hasApprovedMedications = model.ScriptLines?.Any(sl => sl.Status == "Approved") == true;
                Console.WriteLine($"DEBUG: HasApprovedMedications: {hasApprovedMedications}");
                Console.WriteLine($"DEBUG: DoctorId: {model.DoctorId}");

                if (hasApprovedMedications && (!model.DoctorId.HasValue || model.DoctorId.Value == 0))
                {
                    Console.WriteLine("DEBUG: Doctor required but not selected");
                    ModelState.AddModelError("DoctorId", "Doctor is required when approving medications.");
                    TempData["ErrorMessage"] = "Please select a doctor before approving any medications.";
                    await ReloadViewBags();
                    return View(createOrder ? "ProcessAndDispense" : "Edit", model);
                }

                var validationErrors = await ValidateScriptLinesBeforeSave(model.ScriptLines);
                if (validationErrors.Any())
                {
                    Console.WriteLine($"DEBUG: Validation errors: {string.Join(", ", validationErrors)}");
                    TempData["ErrorMessage"] = string.Join(" ", validationErrors);
                    await ReloadViewBags();
                    return View(createOrder ? "ProcessAndDispense" : "Edit", model);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // FIX: Ensure DoctorID is properly set
                    if (model.DoctorId.HasValue && model.DoctorId.Value > 0)
                    {
                        prescription.DoctorID = model.DoctorId.Value;
                        Console.WriteLine($"DEBUG: Updated prescription DoctorID to: {prescription.DoctorID}");
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: WARNING - No valid DoctorID found in model");
                    }

                    // Process script lines
                    foreach (var scriptLineVM in model.ScriptLines.Where(sl => sl.MedicationId > 0))
                    {
                        Console.WriteLine($"DEBUG: Processing ScriptLine - MedicationId: {scriptLineVM.MedicationId}, Status: {scriptLineVM.Status}, ScriptLineId: {scriptLineVM.ScriptLineId}");

                        if (scriptLineVM.ScriptLineId > 0)
                        {
                            await UpdateExistingScriptLine(scriptLineVM);
                            Console.WriteLine($"DEBUG: Updated script line {scriptLineVM.ScriptLineId}, Status: {scriptLineVM.Status}");
                        }
                        else
                        {
                            await CreateNewScriptLine(scriptLineVM, model.Prescr);
                            Console.WriteLine($"DEBUG: Created new script line for medication {scriptLineVM.MedicationId}");
                        }
                    }

                    UpdatePrescriptionStatus(model, prescription);
                    Console.WriteLine($"DEBUG: Updated prescription status to: {prescription.Status}");

                    // Save changes to get updated ScriptLine IDs
                    await _context.SaveChangesAsync();

                    // ORDER CREATION LOGIC - FIXED VERSION
                    if (createOrder && hasApprovedMedications)
                    {
                        Console.WriteLine("DEBUG: Attempting to create order...");
                        var customer = await _context.Customers
                            .FirstOrDefaultAsync(c => c.ApplicationUserId == prescription.ApplicationUserId);

                        Console.WriteLine($"DEBUG: Customer found: {customer != null}, CustomerID: {customer?.CustormerID}");

                        if (customer != null)
                        {
                            var order = await CreateOrderFromPrescription(prescription.PrescriptionID, customer.CustormerID);
                            Console.WriteLine($"DEBUG: Order creation result: {order != null}, OrderNumber: {order?.OrderNumber}");

                            if (order != null)
                            {
                                Console.WriteLine($"DEBUG: SUCCESS - Order created, redirecting to DispenseOrder");
                                await _context.SaveChangesAsync();
                                await transaction.CommitAsync();

                                TempData["SuccessMessage"] = $"Prescription processed successfully! Order {order.OrderNumber} created and ready for dispensing.";
                                return RedirectToAction("DispenseOrder", "PharmacistDispensing", new { id = order.OrderID });
                            }
                            else
                            {
                                Console.WriteLine("DEBUG: Order creation returned null - no order was created");
                                TempData["WarningMessage"] = "Prescription processed but no order was created (no approved medications with valid data).";
                            }
                        }
                        else
                        {
                            Console.WriteLine("DEBUG: Customer not found for prescription");
                            TempData["ErrorMessage"] = "Customer not found for this prescription.";
                        }
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: Order creation skipped - CreateOrder: {createOrder}, HasApprovedMedications: {hasApprovedMedications}");
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    Console.WriteLine("DEBUG: Final save completed, redirecting to ScriptsProcessed Index");

                    TempData["SuccessMessage"] = "Prescription processed successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"DEBUG: Transaction Rollback - Error: {ex.Message}");
                    throw;
                }
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"DEBUG: DbUpdateException: {dbEx.Message}");
                Console.WriteLine($"DEBUG: Inner Exception: {dbEx.InnerException?.Message}");

                var errorMessage = "Database error occurred while saving changes.";
                if (dbEx.InnerException != null)
                {
                    var innerMessage = dbEx.InnerException.Message;
                    if (innerMessage.Contains("FK_Prescriptions_Doctors_DoctorID"))
                    {
                        errorMessage = "Error: Invalid Doctor ID. Please select a valid doctor from the list.";
                    }
                    else if (innerMessage.Contains("FK_ScriptLines_Medications_MedicationID"))
                    {
                        errorMessage = "Error: Invalid Medication ID. Please select valid medications.";
                    }
                    else
                    {
                        errorMessage += $" {innerMessage}";
                    }
                }

                TempData["ErrorMessage"] = errorMessage;
                await ReloadViewBags();
                return View(createOrder ? "ProcessAndDispense" : "Edit", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: General Exception: {ex.Message}");
                Console.WriteLine($"DEBUG: Stack Trace: {ex.StackTrace}");

                TempData["ErrorMessage"] = $"Error processing prescription: {ex.Message}";
                await ReloadViewBags();
                return View(createOrder ? "ProcessAndDispense" : "Edit", model);
            }
        }

        // Create order from prescription (FIXED VERSION with enhanced debugging)
        private async Task<Order> CreateOrderFromPrescription(int prescriptionId, int customerId)
        {
            try
            {
                Console.WriteLine($"DEBUG: CreateOrderFromPrescription started - PrescriptionID: {prescriptionId}, CustomerID: {customerId}");

                var prescription = await _context.Prescriptions
                    .Include(p => p.scriptLines)
                        .ThenInclude(sl => sl.Medications)
                    .FirstOrDefaultAsync(p => p.PrescriptionID == prescriptionId);

                if (prescription == null)
                {
                    Console.WriteLine("DEBUG: Prescription not found in CreateOrderFromPrescription");
                    throw new Exception("Prescription not found");
                }

                Console.WriteLine($"DEBUG: Found prescription with {prescription.scriptLines.Count} script lines");

                var approvedScriptLines = prescription.scriptLines.Where(sl => sl.Status == "Approved").ToList();
                Console.WriteLine($"DEBUG: Approved script lines: {approvedScriptLines.Count}");

                // DEBUG: Log each approved script line
                foreach (var sl in approvedScriptLines)
                {
                    Console.WriteLine($"DEBUG: Approved ScriptLine - ID: {sl.ScriptLineID}, MedicationID: {sl.MedicationID}, Status: {sl.Status}, Medications: {sl.Medications != null}");
                }

                if (!approvedScriptLines.Any())
                {
                    Console.WriteLine("DEBUG: No approved script lines found for order creation");
                    return null;
                }

                string orderNumber = await GenerateUniqueOrderNumber();
                Console.WriteLine($"DEBUG: Generated order number: {orderNumber}");

                var order = new Order
                {
                    CustomerID = customerId,
                    OrderDate = DateTime.Now,
                    Status = "Ordered",
                    VAT = 15,
                    OrderNumber = orderNumber
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Get OrderID
                Console.WriteLine($"DEBUG: Order created with ID: {order.OrderID}");

                decimal subtotal = 0;
                bool hasValidMedications = false;

                foreach (var scriptLine in approvedScriptLines)
                {
                    Console.WriteLine($"DEBUG: Creating order line for medication {scriptLine.MedicationID}, Quantity: {scriptLine.Quantity}");

                    if (scriptLine.Medications == null)
                    {
                        Console.WriteLine($"DEBUG: Medications is null for ScriptLine {scriptLine.ScriptLineID}, skipping...");
                        continue;
                    }

                    if (scriptLine.Medications.CurrentPrice <= 0)
                    {
                        Console.WriteLine($"DEBUG: Invalid price for medication {scriptLine.MedicationID}, skipping...");
                        continue;
                    }

                    var orderLine = new OrderLine
                    {
                        OrderID = order.OrderID,
                        ScriptLineID = scriptLine.ScriptLineID,
                        MedicationID = scriptLine.MedicationID,
                        Quantity = scriptLine.Quantity,
                        ItemPrice = (int)scriptLine.Medications.CurrentPrice,
                        Status = "Pending"
                    };

                    _context.OrderLines.Add(orderLine);
                    subtotal += scriptLine.Medications.CurrentPrice * scriptLine.Quantity;
                    hasValidMedications = true;
                    Console.WriteLine($"DEBUG: Order line created, subtotal now: {subtotal}");
                }

                if (!hasValidMedications)
                {
                    Console.WriteLine("DEBUG: No valid medications with prices found");
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                    return null;
                }

                decimal totalDue = subtotal + (subtotal * order.VAT / 100);
                order.TotalDue = totalDue.ToString("F2");

                Console.WriteLine($"DEBUG: Order completed successfully, Subtotal: {subtotal:F2}, TotalDue: {order.TotalDue}");
                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: ERROR in CreateOrderFromPrescription: {ex.Message}");
                Console.WriteLine($"DEBUG: Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        // Generate unique order number with ORD-P-RRRR format (P for Processed)
        private async Task<string> GenerateUniqueOrderNumber()
        {
            string orderNumber;
            bool isUnique;
            int maxAttempts = 5;
            int attempts = 0;

            do
            {
                string randomPart = _random.Next(1000, 10000).ToString();
                orderNumber = $"ORD-P-{randomPart}";

                isUnique = !await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber);
                attempts++;

            } while (!isUnique && attempts < maxAttempts);

            if (!isUnique)
            {
                string timestamp = DateTime.Now.ToString("HHmmss");
                orderNumber = $"ORD-P-{timestamp}";
            }

            return orderNumber;
        }

        // POST: Add new doctor via AJAX
        [HttpPost]
        public async Task<JsonResult> AddNewDoctor([FromBody] Doctor newDoctor)
        {
            try
            {
                if (string.IsNullOrEmpty(newDoctor.Name) || string.IsNullOrEmpty(newDoctor.Surname) ||
                    string.IsNullOrEmpty(newDoctor.HealthCouncilRegistrationNumber) || string.IsNullOrEmpty(newDoctor.ContactNumber))
                {
                    return Json(new { success = false, message = "All fields are required" });
                }

                var existingDoctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.HealthCouncilRegistrationNumber == newDoctor.HealthCouncilRegistrationNumber);

                if (existingDoctor != null)
                {
                    return Json(new { success = false, message = "Doctor with this registration number already exists" });
                }

                _context.Doctors.Add(newDoctor);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    doctorId = newDoctor.DoctorID,
                    doctorName = $"{newDoctor.Name} {newDoctor.Surname}",
                    fullDisplay = $"{newDoctor.Name} {newDoctor.Surname} ({newDoctor.HealthCouncilRegistrationNumber})"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error saving doctor: {ex.Message}" });
            }
        }

        private async Task<List<string>> ValidateScriptLinesBeforeSave(List<ScriptLineVM> scriptLines)
        {
            var errors = new List<string>();

            if (scriptLines == null || !scriptLines.Any())
            {
                errors.Add("No medication lines to process.");
                return errors;
            }

            var validMedicationIds = await _context.Medications.Select(m => m.MedcationID).ToListAsync();

            foreach (var scriptLine in scriptLines.Where(sl => sl.MedicationId > 0))
            {
                if (!validMedicationIds.Contains(scriptLine.MedicationId))
                {
                    errors.Add($"Medication ID {scriptLine.MedicationId} does not exist in database.");
                    continue;
                }

                if (scriptLine.Status == "Rejected" && string.IsNullOrWhiteSpace(scriptLine.RejectionReason))
                {
                    errors.Add("Rejection reason is required for rejected medications.");
                }
            }

            return errors;
        }

        private async Task UpdateExistingScriptLine(ScriptLineVM scriptLineVM)
        {
            var existingScriptLine = await _context.ScriptLines
                .FirstOrDefaultAsync(sl => sl.ScriptLineID == scriptLineVM.ScriptLineId);

            if (existingScriptLine != null)
            {
                existingScriptLine.MedicationID = scriptLineVM.MedicationId;
                existingScriptLine.Quantity = scriptLineVM.Quantity;
                existingScriptLine.Instructions = scriptLineVM.Instructions ?? string.Empty;
                existingScriptLine.Repeats = scriptLineVM.IsRepeat ? 3 : 0;
                existingScriptLine.RepeatsLeft = scriptLineVM.RepeatsLeft;
                existingScriptLine.Status = scriptLineVM.Status ?? "Pending";
                existingScriptLine.RejectionReason = scriptLineVM.RejectionReason;

                UpdateScriptLineDates(existingScriptLine, scriptLineVM.Status);
            }
        }

        private async Task CreateNewScriptLine(ScriptLineVM scriptLineVM, int prescriptionId)
        {
            var newScriptLine = new ScriptLine
            {
                PrescriptionID = prescriptionId,
                MedicationID = scriptLineVM.MedicationId,
                Quantity = scriptLineVM.Quantity,
                Instructions = scriptLineVM.Instructions ?? string.Empty,
                Repeats = scriptLineVM.IsRepeat ? 3 : 0,
                RepeatsLeft = scriptLineVM.RepeatsLeft,
                Status = scriptLineVM.Status ?? "Pending",
                RejectionReason = scriptLineVM.RejectionReason
            };

            UpdateScriptLineDates(newScriptLine, scriptLineVM.Status);
            _context.ScriptLines.Add(newScriptLine);
        }

        private void UpdateScriptLineDates(ScriptLine scriptLine, string status)
        {
            if (status == "Approved")
            {
                scriptLine.ApprovedDate = DateTime.Now;
                scriptLine.RejectedDate = null;
            }
            else if (status == "Rejected")
            {
                scriptLine.RejectedDate = DateTime.Now;
                scriptLine.ApprovedDate = null;
            }
            else
            {
                scriptLine.ApprovedDate = null;
                scriptLine.RejectedDate = null;
            }
        }

        private void UpdatePrescriptionStatus(CustomerScriptsVM model, Prescription prescription)
        {
            var approvedLines = model.ScriptLines.Count(sl => sl.Status == "Approved");
            var rejectedLines = model.ScriptLines.Count(sl => sl.Status == "Rejected");

            if (approvedLines > 0 && rejectedLines > 0)
                prescription.Status = "Partially Processed";
            else if (approvedLines > 0)
                prescription.Status = "Processed";
            else if (rejectedLines > 0)
                prescription.Status = "Rejected";
            else
                prescription.Status = "Pending";
        }

        // Download prescription file
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var prescription = await _context.Prescriptions
                    .FirstOrDefaultAsync(p => p.PrescriptionID == id);

                if (prescription == null || prescription.Script == null)
                    return NotFound();

                return File(prescription.Script, "application/pdf", $"Prescription_{id}.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error downloading prescription: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // API: Check for allergy conflicts
        [HttpGet]
        public async Task<JsonResult> CheckAllergyConflicts(int prescriptionId, int medicationId)
        {
            try
            {
                var prescription = await _context.Prescriptions
                    .Include(p => p.ApplicationUser)
                    .FirstOrDefaultAsync(p => p.PrescriptionID == prescriptionId);

                if (prescription == null)
                    return Json(new { hasConflicts = false, conflicts = new string[0] });

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == prescription.ApplicationUserId);

                if (customer == null)
                    return Json(new { hasConflicts = false, conflicts = new string[0] });

                var customerAllergies = await _context.Custormer_Allergies
                    .Where(ca => ca.CustomerID == customer.CustormerID)
                    .Include(ca => ca.Active_Ingredient)
                    .Select(ca => ca.Active_Ingredient.Name)
                    .ToListAsync();

                var medicationIngredients = await _context.Medication_Ingredients
                    .Where(mi => mi.MedicationID == medicationId)
                    .Include(mi => mi.Active_Ingredients)
                    .Select(mi => mi.Active_Ingredients.Name)
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

        // API: Get medication details for display
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

                string stockStatus = "Normal";
                if (medication.QuantityOnHand == 0)
                {
                    stockStatus = "OutOfStock";
                }
                else if (medication.QuantityOnHand <= 50 && medication.QuantityOnHand <= medication.ReOrderLevel)
                {
                    stockStatus = "VeryLowStock";
                }
                else if (medication.QuantityOnHand <= medication.ReOrderLevel)
                {
                    stockStatus = "LowStock";
                }

                return Json(new
                {
                    medicationName = medication.MedicationName,
                    activeIngredients = string.Join(", ", activeIngredients),
                    stock = medication.QuantityOnHand,
                    reorderLevel = medication.ReOrderLevel,
                    dosageForm = medication.DosageForm?.DosageFormName,
                    schedule = medication.Schedule,
                    price = medication.CurrentPrice,
                    stockStatus = stockStatus
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Error loading medication: {ex.Message}" });
            }
        }

        // Helper method to reload ViewBags
        private async Task ReloadViewBags()
        {
            ViewBag.Doctors = await _context.Doctors.ToListAsync();
            ViewBag.Medications = await _context.Medications
                .Include(m => m.DosageForm)
                .Include(m => m.Medication_Ingredients)
                .ThenInclude(mi => mi.Active_Ingredients)
                .ToListAsync();
        }

        // GET: ProcessedScripts - Show all processed prescriptions
        public async Task<IActionResult> ProcessedScripts()
        {
            try
            {
                var processedPrescriptions = await _context.Prescriptions
                    .Include(p => p.ApplicationUser)
                    .Include(p => p.Doctors)
                    .Include(p => p.scriptLines)
                    .Where(p => p.Status == "Processed" || p.Status == "Partially Processed" || p.Status == "Rejected")
                    .OrderByDescending(p => p.DateIssued)
                    .ToListAsync();

                return View(processedPrescriptions);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading processed prescriptions: {ex.Message}";
                return View(new List<Prescription>());
            }
        }

        // GET: ProcessedScripts Details - View details of a processed prescription
        public async Task<IActionResult> ProcessedDetails(int id)
        {
            try
            {
                var prescription = await _context.Prescriptions
                    .Include(p => p.ApplicationUser)
                    .Include(p => p.Doctors)
                    .Include(p => p.scriptLines)
                        .ThenInclude(sl => sl.Medications)
                    .FirstOrDefaultAsync(p => p.PrescriptionID == id);

                if (prescription == null)
                {
                    TempData["ErrorMessage"] = "Processed prescription not found";
                    return RedirectToAction(nameof(ProcessedScripts));
                }

                var viewModel = new ProcessedPrescriptionVM
                {
                    PrescriptionID = prescription.PrescriptionID,
                    PatientName = $"{prescription.ApplicationUser?.Name} {prescription.ApplicationUser?.Surname}",
                    IDNumber = prescription.ApplicationUser?.IDNumber ?? "",
                    Email = prescription.ApplicationUser?.Email ?? "",
                    DateIssued = prescription.DateIssued,
                    DoctorName = prescription.Doctors != null ?
                        $"{prescription.Doctors.Name} {prescription.Doctors.Surname}" : "Not Assigned",
                    Status = prescription.Status ?? "Unknown",
                    ScriptLines = prescription.scriptLines.Select(sl => new ProcessedScriptLineVM
                    {
                        MedicationName = sl.Medications?.MedicationName ?? "Unknown",
                        Quantity = sl.Quantity,
                        Instructions = sl.Instructions ?? "",
                        Status = sl.Status ?? "Pending",
                        RejectionReason = sl.RejectionReason,
                        ApprovedDate = sl.ApprovedDate,
                        RejectedDate = sl.RejectedDate
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading prescription details: {ex.Message}";
                return RedirectToAction(nameof(ProcessedScripts));
            }
        }
    }
}