using IbhayiPharmacy.Data;
using IbhayiPharmacy.Models;
using IbhayiPharmacy.Models.PharmacistVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

public class tempScriptController : Controller
{
    private readonly ApplicationDbContext _context;

    public tempScriptController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    // GET: Prescription/Create
    public async Task<IActionResult> Create()
    {
        var viewModel = new PrescriptionViewModel
        {
            PrescriptionDate = DateTime.Now,
            AvailableDoctors = await _context.Doctors.ToListAsync(),
            AvailableMedications = await _context.Medications
                .Include(m => m.Medication_Ingredients)
                .ThenInclude(mi => mi.Active_Ingredients)
                .ToListAsync(),
            ScriptLines = new List<ScriptLineViewModel> { new ScriptLineViewModel() }
        };

        return View(viewModel);
    }

    // POST: Prescription/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PrescriptionViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.AvailableDoctors = await _context.Doctors.ToListAsync();
            viewModel.AvailableMedications = await _context.Medications
                .Include(m => m.Medication_Ingredients)
                .ThenInclude(mi => mi.Active_Ingredients)
                .ToListAsync();
            return View(viewModel);
        }

        try
        {
            var newScript = new NewScript
            {
                DateIssued = viewModel.PrescriptionDate,
                Script = null, // PDF can be uploaded separately
                scriptLines = new List<PresScriptLine>()
            };

            foreach (var line in viewModel.ScriptLines)
            {
                var scriptLine = new PresScriptLine
                {
                    MedicationID = line.MedicationID,
                    Quantity = line.Quantity,
                    Instructions = line.Instructions,
                    Repeats = line.Repeats,
                    RepeatsLeft = line.Repeats
                };
                newScript.scriptLines.Add(scriptLine);
            }
            _context.NewScripts.Add(newScript);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Prescription created successfully!";
            return RedirectToAction(nameof(Details), new { id = newScript.PrescriptionID });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            viewModel.AvailableDoctors = await _context.Doctors.ToListAsync();
            viewModel.AvailableMedications = await _context.Medications
                .Include(m => m.Medication_Ingredients)
                .ThenInclude(mi => mi.Active_Ingredients)
                .ToListAsync();
            return View(viewModel);
        }
    }

    // GET: Prescription/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var script = await _context.NewScripts
            .Include(s => s.scriptLines)
            .ThenInclude(m => m.Medications)
            .FirstOrDefaultAsync(s => s.PrescriptionID == id);

        if (script == null) return NotFound();

        return View(script); // Razor view can access ScriptLines and Script (PDF)
    }

    // GET: Prescription/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var script = await _context.NewScripts
            .Include(s => s.scriptLines)
            .FirstOrDefaultAsync(s => s.PrescriptionID == id);

        if (script == null) return NotFound();

        var viewModel = new PrescriptionViewModel
        {
            PrescriptionDate = script.DateIssued,
            AvailableDoctors = await _context.Doctors.ToListAsync(),
            AvailableMedications = await _context.Medications
                .Include(m => m.Medication_Ingredients)
                .ThenInclude(mi => mi.Active_Ingredients)
                .ToListAsync(),
            ScriptLines = new List<ScriptLineViewModel>()
        };

        foreach (var line in script.scriptLines)
        {
            viewModel.ScriptLines.Add(new ScriptLineViewModel
            {
                MedicationID = line.MedicationID,
                Quantity = line.Quantity,
                Instructions = line.Instructions,
                Repeats = line.Repeats,
                RepeatsLeft = line.RepeatsLeft
            });
        }
        return View(viewModel);
    }

    // POST: Prescription/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PrescriptionViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.AvailableDoctors = await _context.Doctors.ToListAsync();
            viewModel.AvailableMedications = await _context.Medications
                .Include(m => m.Medication_Ingredients)
                .ThenInclude(mi => mi.Active_Ingredients)
                .ToListAsync();
            return View(viewModel);
        }

        var script = await _context.NewScripts
            .Include(s => s.scriptLines)
            .FirstOrDefaultAsync(s => s.PrescriptionID == id);

        if (script == null) return NotFound();

        script.DateIssued = viewModel.PrescriptionDate;
        //script.scriptLines = viewModel.ScriptLines;

        //This is where it starts
        foreach (var lineVM in viewModel.ScriptLines)
        {
            if (lineVM.ScriptLineID > 0)
            {
                // Existing line — update it
                var existingLine = script.scriptLines.FirstOrDefault(sl => sl.ScriptLineID == lineVM.ScriptLineID);
                if (existingLine != null)
                {
                    existingLine.MedicationID = lineVM.MedicationID;
                    existingLine.Quantity = lineVM.Quantity;
                    existingLine.Instructions = lineVM.Instructions;
                    existingLine.Repeats = lineVM.Repeats;
                    existingLine.RepeatsLeft = lineVM.RepeatsLeft;
                }
            }
            else
            {
                // New line — add it
                var newLine = new PresScriptLine
                {
                    PrescriptionID = script.PrescriptionID,
                    MedicationID = lineVM.MedicationID,
                    Quantity = lineVM.Quantity,
                    Instructions = lineVM.Instructions,
                    Repeats = lineVM.Repeats,
                    RepeatsLeft = lineVM.RepeatsLeft
                };
                script.scriptLines.Add(newLine);  // Add to navigation property
            }
        }

        // Optional: Remove lines that were deleted in the view
        var linesToRemove = script.scriptLines
            .Where(sl => !viewModel.ScriptLines.Any(vm => vm.ScriptLineID == sl.ScriptLineID))
            .ToList();

        foreach (var line in linesToRemove)
        {
            _context.PresScriptLines.Remove(line);
        }

        //This is where it ends

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Prescription updated successfully!";
        return RedirectToAction(nameof(Details), new { id = script.PrescriptionID });
    }

    //AJAX: Get doctors
   [HttpGet]
    public async Task<IActionResult> GetDoctors()
    {
        var doctors = await _context.Doctors.ToListAsync();
        return Json(doctors);
    }

    // AJAX: Get medication details
    [HttpGet]
    public async Task<IActionResult> GetMedicationDetails(int id)
    {
        var medication = await _context.Medications
            .Include(m => m.Medication_Ingredients)
            .ThenInclude(mi => mi.Active_Ingredients)
            .FirstOrDefaultAsync(m => m.MedcationID == id);

        if (medication == null) return NotFound();

        return Json(medication); // Razor view can extract ingredients, schedule, price, etc.
    }
}
