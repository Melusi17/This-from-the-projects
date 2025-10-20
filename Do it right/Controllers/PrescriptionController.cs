using IbhayiPharmacy.Data;
using IbhayiPharmacy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IbhayiPharmacy.Controllers
{
    [Authorize(Policy = "Customer")]
    public class PrescriptionController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;

        public PrescriptionController(ApplicationDbContext db, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
           
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var prescriptions = _db.Prescriptions
                .Where(p => p.ApplicationUserId==userId)
                .ToList();

            return View(prescriptions);
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
               
                return View(new Prescription());
            }

          
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var prescription = _db.Prescriptions
                .FirstOrDefault(p => p.PrescriptionID == id && p.ApplicationUserId == userId);

            if (prescription == null)
                return NotFound();

            return View(prescription);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Prescription prescription, IFormFile? file)
        {
            if (!ModelState.IsValid)
                return View(prescription);

           
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            prescription.ApplicationUserId = userId;

           
            if (file != null && file.Length > 0)
            {
                if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
                    return BadRequest("Only PDF files are allowed.");

                string uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "Documents/Scripts");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                string filePath = Path.Combine(uploadPath, file.FileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    prescription.Script = ms.ToArray();
                }
            }

            if (prescription.PrescriptionID == 0)
            {
                

                _db.Prescriptions.Add(prescription);
            }
            else
            {
               
                var existing = _db.Prescriptions
                    .FirstOrDefault(p => p.PrescriptionID == prescription.PrescriptionID && p.ApplicationUserId == userId);

                if (existing == null)
                    return NotFound();

                _db.Entry(existing).CurrentValues.SetValues(prescription);
            }

            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
