using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models
{
    public class Medication
    {
        [Key]
        public int MedcationID { get; set; }

        [ForeignKey("DosageFormID")]
        public int DosageFormID { get; set; }
        public DosageForm DosageForm { get; set; }

        [Required]
        public string? MedicationName { get; set; }

        [Required]
        public string? Schedule { get; set; }

        [Required]
        public int CurrentPrice { get; set; }

        [ForeignKey("SupplierID")]
        public int SupplierID { get; set; }

        public Supplier Supplier { get; set; }

        [Required]
        public int ReOrderLevel { get; set; }

        public int QuantityOnHand { get; set; }

        [ValidateNever]
        public List<Medication_Ingredient> Medication_Ingredients { get; set; } = new List<Medication_Ingredient>();

    }
}
