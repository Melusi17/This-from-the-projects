using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models
{
    public class OrderLine
    {
        [Key]
        public int OrderLineID { get; set; }

        [ForeignKey("OrderID")]
        public int OrderID { get; set; }
        public Order Order { get; set; } = null!;

        [ForeignKey("MedicationID")]
        public int MedicationID { get; set; }
        public Medication Medications { get; set; } = null!;

        [ForeignKey("ScriptLineID")]
        public int ScriptLineID { get; set; }
        public ScriptLine ScriptLine { get; set; } = null!;

        [Required]
        public int ItemPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        // NEW: Status for order line processing
        public string Status { get; set; } = "Pending";

        // NEW: Reason for rejection if applicable
        public string? RejectionReason { get; set; }
    }
}