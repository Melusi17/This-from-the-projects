using Microsoft.AspNetCore.Mvc;
using IbhayiPharmacy.Models;
using IbhayiPharmacy.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace PharmMan.Controllers
{
    public class PharmacyManagerController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PharmacyManagerController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult PharmacyInfo()
        {
            IEnumerable<Pharmacy> obj = _db.Pharmacies;
            return View(obj);
        }
        [HttpGet]
        public IActionResult AddPharmacyInfo()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPharmacyInfo(Pharmacy model)
        {
            if (ModelState.IsValid)
            {
                _db.Pharmacies.Add(model);
                _db.SaveChanges();
                return RedirectToAction("PharmacyInfo");
            }
            return View(model);
        }






        public IActionResult ActiveIngredients()
        {
            IEnumerable<Active_Ingredient> active = _db.Active_Ingredients.ToList();
            return View(active);
        }

        public IActionResult AddActiveIngredients()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddActiveIngredients(Active_Ingredient active)
        {
            if (ModelState.IsValid)
            {
                _db.Active_Ingredients.Add(active);
                _db.SaveChanges();
                return RedirectToAction("ActiveIngredients");
            }

            return View(active);
        }






        //Dosage Forms
        public IActionResult DosageForms()
        {
            IEnumerable<DosageForm> forms = _db.DosageForms;
            return View(forms);
        }
        public IActionResult AddDosageForms()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddDosageForms(DosageForm form)
        {
            if (ModelState.IsValid)
            {
                _db.DosageForms.Add(form);
                _db.SaveChanges();
                return RedirectToAction("DosageForms");
            }
            return View(form);
        }






        //Supplier
        public IActionResult MedicationSuppliers()
        {
            IEnumerable<Supplier> supplier = _db.Suppliers;
            return View(supplier);
        }
        [HttpGet]
        public IActionResult AddSuppliers()
        {

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddSuppliers(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                _db.Suppliers.Add(supplier);
                _db.SaveChanges();
                return RedirectToAction("MedicationSuppliers");
            }
            return View(supplier);
        }



        [HttpGet]
        public IActionResult ManageMedication()
        {
            var med = _db.Medications
         .Include(m => m.DosageForm)
         .Include(m => m.Supplier)
         .Include(m => m.Medication_Ingredients)
             .ThenInclude(mi => mi.Active_Ingredients) // ✅ this line is key
         .ToList();

            return View(med);
        }

        public IActionResult AddMedication()
        {
            ViewBag.DosageForms = new SelectList(_db.DosageForms, "DosageFormID", "DosageFormName");
            ViewBag.Suppliers = new SelectList(_db.Suppliers, "SupplierID", "SupplierName");
            ViewBag.ActiveIngredients = _db.Active_Ingredients.ToList(); // optional for multi-select
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMedication(Medication med, List<MedicationIngredientVM> Ingredients)
        {
            if (ModelState.IsValid)
            {
                foreach (var ing in Ingredients)
                {
                    med.Medication_Ingredients.Add(new Medication_Ingredient
                    {
                        Active_IngredientID = ing.Active_IngredientID,
                        Strength = ing.Strength
                    });
                }

                _db.Medications.Add(med);
                _db.SaveChanges();
                return RedirectToAction("ManageMedication");
            }

            ViewBag.DosageForms = new SelectList(_db.DosageForms, "DosageFormID", "DosageFormName");
            ViewBag.Suppliers = new SelectList(_db.Suppliers, "SupplierID", "SupplierName");
            ViewBag.ActiveIngredients = _db.Active_Ingredients.ToList();
            return View(med);
        }














        //Doctors
        public IActionResult ManageDoctor()
        {
            IEnumerable<Doctor> doctors = _db.Doctors;
            return View(doctors);
        }
        public IActionResult AddDoctor()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddDoctor(Doctor doc)
        {
            if (ModelState.IsValid)
            {
                _db.Doctors.Add(doc);
                _db.SaveChanges();
                return RedirectToAction("ManageDoctor");
            }
            return View(doc);
        }







        //Pharmacists
        public IActionResult ManagePharmacists()
        {
            IEnumerable<Pharmacist> pharmacist = _db.Pharmacists.Include(p => p.ApplicationUser).ToList();
            return View(pharmacist);
        }
        public IActionResult AddPharmacists()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPharmacists(Pharmacist pham)
        {

            _db.Pharmacists.Add(pham);
            _db.SaveChanges();
            return RedirectToAction("ManagePharmacists");


        }


        //Orders
        public IActionResult ManageOrders()
        {
            // Include customer, pharmacist, and order lines with medications & suppliers
            var orders = _db.Orders
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Medications)
                        .ThenInclude(m => m.Supplier)
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }
        public IActionResult Orders()
        {
            return View();
        }
      

        public IActionResult Reports()
        {
            return View();
        }



    }
}
