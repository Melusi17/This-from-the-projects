using IbhayiPharmacy.Data;
using IbhayiPharmacy.Models;
using IbhayiPharmacy.Models.PharmacistVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ScriptsProcessedController : Controller
{
    private readonly ApplicationDbContext _context;

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
                .Include(p => p.Doctors) // Include doctor information
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

    // GET: Edit - Show form to process prescription
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.ApplicationUser)
                .Include(p => p.Doctors) // Include the assigned doctor
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
                // Set the doctor from prescription level
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
                    // Doctor information removed from script line level
                    CanBeApproved = prescription.DoctorID.HasValue // Set approval capability
                }).ToList()
            };

            // Add empty row if no script lines
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

    // POST: Edit - Save to database
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CustomerScriptsVM model)
    {
        try
        {
            // DEBUG: Log incoming data
            Console.WriteLine($"=== DEBUG: Starting Edit POST ===");
            Console.WriteLine($"Prescription ID: {model.Prescr}");
            Console.WriteLine($"Doctor ID: {model.DoctorId}");
            Console.WriteLine($"ScriptLines Count: {model.ScriptLines?.Count}");

            if (model.ScriptLines != null)
            {
                foreach (var line in model.ScriptLines)
                {
                    Console.WriteLine($"Line {line.ScriptLineId}: MedId={line.MedicationId}, Status={line.Status}");
                }
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fix the validation errors below.";
                await ReloadViewBags();
                return View(model);
            }

            // Check if Prescription Exists
            var prescription = await _context.Prescriptions
                .FirstOrDefaultAsync(p => p.PrescriptionID == model.Prescr);

            if (prescription == null)
            {
                TempData["ErrorMessage"] = "Prescription not found";
                return RedirectToAction(nameof(Index));
            }

            // Validate at least one medication is processed
            var hasMedications = model.ScriptLines?.Any(sl => sl.MedicationId > 0) == true;
            if (!hasMedications)
            {
                ModelState.AddModelError("", "Please add at least one medication to process the prescription.");
                TempData["ErrorMessage"] = "Please add at least one medication to process the prescription.";
                await ReloadViewBags();
                return View(model);
            }

            // CRITICAL: Validate doctor is selected if any medications are approved
            var hasApprovedMedications = model.ScriptLines?.Any(sl => sl.Status == "Approved") == true;
            if (hasApprovedMedications && (!model.DoctorId.HasValue || model.DoctorId.Value == 0))
            {
                ModelState.AddModelError("DoctorId", "Doctor is required when approving medications.");
                TempData["ErrorMessage"] = "Please select a doctor before approving any medications.";
                await ReloadViewBags();
                return View(model);
            }

            // Validate all foreign keys before saving
            var validationErrors = await ValidateScriptLinesBeforeSave(model.ScriptLines);
            if (validationErrors.Any())
            {
                TempData["ErrorMessage"] = string.Join(" ", validationErrors);
                await ReloadViewBags();
                return View(model);
            }

            // UPDATE PRESCRIPTION LEVEL DATA
            prescription.DoctorID = model.DoctorId; // Set the single doctor for entire prescription
            Console.WriteLine($"✅ Set DoctorID {model.DoctorId} for Prescription {prescription.PrescriptionID}");

            // Process script lines
            foreach (var scriptLineVM in model.ScriptLines.Where(sl => sl.MedicationId > 0))
            {
                if (scriptLineVM.ScriptLineId > 0)
                {
                    await UpdateExistingScriptLine(scriptLineVM);
                }
                else
                {
                    await CreateNewScriptLine(scriptLineVM, model.Prescr);
                }
            }

            // Update prescription status based on medication outcomes
            UpdatePrescriptionStatus(model, prescription);

            // Save changes
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Prescription processed successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException dbEx)
        {
            // Enhanced error handling for FK constraints
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
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error processing prescription: {ex.Message}";
            await ReloadViewBags();
            return View(model);
        }
    }

    // CRITICAL FIX: Validate all foreign keys before saving
    private async Task<List<string>> ValidateScriptLinesBeforeSave(List<ScriptLineVM> scriptLines)
    {
        var errors = new List<string>();

        if (scriptLines == null || !scriptLines.Any())
        {
            errors.Add("No medication lines to process.");
            return errors;
        }

        // Get all valid IDs upfront for performance
        var validMedicationIds = await _context.Medications.Select(m => m.MedcationID).ToListAsync();

        foreach (var scriptLine in scriptLines.Where(sl => sl.MedicationId > 0))
        {
            // Validate medication exists
            if (!validMedicationIds.Contains(scriptLine.MedicationId))
            {
                errors.Add($"Medication ID {scriptLine.MedicationId} does not exist in database.");
                continue;
            }

            // Validate rejection reason if rejected
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
            // Update properties - NO DoctorID at script line level anymore
            existingScriptLine.MedicationID = scriptLineVM.MedicationId;
            existingScriptLine.Quantity = scriptLineVM.Quantity;
            existingScriptLine.Instructions = scriptLineVM.Instructions ?? string.Empty;
            existingScriptLine.Repeats = scriptLineVM.IsRepeat ? 3 : 0;
            existingScriptLine.RepeatsLeft = scriptLineVM.RepeatsLeft;
            existingScriptLine.Status = scriptLineVM.Status ?? "Pending";
            existingScriptLine.RejectionReason = scriptLineVM.RejectionReason;

            UpdateScriptLineDates(existingScriptLine, scriptLineVM.Status);

            Console.WriteLine($"✅ Updated ScriptLine {existingScriptLine.ScriptLineID}: Status={existingScriptLine.Status}, MedicationID={existingScriptLine.MedicationID}");
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
            // NO DoctorID - now at prescription level only
        };

        // Set dates based on status
        UpdateScriptLineDates(newScriptLine, scriptLineVM.Status);

        _context.ScriptLines.Add(newScriptLine);
        Console.WriteLine($"✅ Created new ScriptLine: PrescriptionID={newScriptLine.PrescriptionID}, MedicationID={newScriptLine.MedicationID}, Status={newScriptLine.Status}");
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

        Console.WriteLine($"✅ Updated prescription {prescription.PrescriptionID} status to: {prescription.Status}");
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

            var isLowStock = medication.QuantityOnHand <= medication.ReOrderLevel + 10;

            return Json(new
            {
                medicationName = medication.MedicationName,
                activeIngredients = string.Join(", ", activeIngredients),
                stock = medication.QuantityOnHand,
                reorderLevel = medication.ReOrderLevel,
                dosageForm = medication.DosageForm?.DosageFormName,
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

    // NEW: Debug endpoint to check database state
    [HttpGet]
    public async Task<JsonResult> DebugDatabase()
    {
        try
        {
            var doctors = await _context.Doctors
                .Select(d => new { d.DoctorID, d.Name, d.Surname })
                .ToListAsync();

            var medications = await _context.Medications
                .Select(m => new { m.MedcationID, m.MedicationName })
                .Take(10)
                .ToListAsync();

            return Json(new
            {
                doctors = doctors,
                medications = medications
            });
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
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
                .Include(p => p.Doctors) // Include doctor information
                .Include(p => p.scriptLines) // Include script lines to check processing status
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

            // Create a view model similar to CustomerScriptsVM but for view-only
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