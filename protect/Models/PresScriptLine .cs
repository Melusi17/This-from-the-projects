using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models
{
    public class PresScriptLine
    {
        [Key]
        public int ScriptLineID { get; set; }

        //Data Already there
        [ForeignKey("PrescriptionID")]
        public int PrescriptionID { get; set; }

        public Prescription Prescriptions { get; set; }//PDF

        //Populates
        [ForeignKey("MedicationID")]
        public int MedicationID { get; set; }

        public Medication Medications { get; set; }

        
        public int Quantity { get; set; }

        public string DispenseStatus { get; set; } = "Pending";

        [Required]
        public string? Instructions { get; set; }

        [Required]
        public int Repeats { get; set; }

        [Required]
        public int RepeatsLeft { get; set; }

    }
}
