using IbhayiPharmacy.Models;
using System.ComponentModel.DataAnnotations;

namespace IbhayiPharmacy.Models.PharmacistVM
{
    public class CollectionTrackingVM
    {
        public List<Order> Orders { get; set; } = new List<Order>();
        public int ReadyForCollectionCount { get; set; }
        public int CollectedCount { get; set; }
        public int TotalOrders { get; set; }
    }

    public class OrderCollectionItemVM
    {
        public int OrderID { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerIDNumber { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string TotalDue { get; set; } = string.Empty;
        public string PharmacistName { get; set; } = string.Empty;
        public List<OrderLineCollectionVM> OrderLines { get; set; } = new List<OrderLineCollectionVM>();
        public bool HasRepeats { get; set; }
        public int RepeatsLeft { get; set; }
    }

    public class OrderLineCollectionVM
    {
        public string MedicationName { get; set; } = string.Empty;
        public string DosageForm { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Schedule { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
    }
}