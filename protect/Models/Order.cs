using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        [ForeignKey("CustomerID")]
        public int CustomerID { get; set; }
        public Customer Customer { get; set; } = null!;

        [ForeignKey("PharmacistID")]
        public int? PharmacistID { get; set; }
        [ValidateNever]
        public Pharmacist Pharmacist { get; set; } = null!;

        public string? OrderNumber { get; set; }//(e.g., "ORD-20251001-001")
        public DateTime OrderDate { get; set; }

        public string Status { get; set; } = "Ordered";

        public string TotalDue { get; set; } = string.Empty;

        public int VAT { get; set; }    

        // Add this navigation property
        public virtual ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    }
}