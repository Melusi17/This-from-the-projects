using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models
{
    public class ScriptLine
    {
        [Key]
        public int ScriptLineID { get; set; }

        [ForeignKey("PrescriptionID")]
        public int PrescriptionID { get; set; }

        public Prescription Prescriptions { get; set; }

        [ForeignKey("MedicationID")]
        public int MedicationID { get; set; }

        public Medication Medications { get; set; }

        //[ForeignKey("PharmacistID")]
        //public int PharmacistID { get; set; }

      
        public int Quantity { get; set; }

        public string Instructions { get; set; }

       
        public int Repeats { get; set; }

   
        public int RepeatsLeft { get; set; }

        
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string? RejectionReason { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? RejectedDate { get; set; }

    }
}
