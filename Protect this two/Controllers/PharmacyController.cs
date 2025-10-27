using Microsoft.AspNetCore.Mvc;

namespace IbhayiPharmacy.Controllers
{
    public class PharmacyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult loadPrescription()
        {
            return View();
        }

        public IActionResult LoadAndDispense() 
        {
            return View();
        }

        public IActionResult Dispensary()
        {
            return View();
        }

        public IActionResult NewDispensary()
        {
            return View();
        }

        public IActionResult UnprocessedScripts()
        {
            return View();
        }

        public IActionResult dispensedOrders()
        {
            return View();
        }

        public IActionResult processedOrders()
        {
            return View();
        }
    }
}
