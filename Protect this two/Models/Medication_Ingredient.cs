using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models
{
    public class Medication_Ingredient
    {
        [Key]
        public int Medication_IngredientID { get; set; }
        [ForeignKey("MedicationID")]
        public int MedicationID { get; set; }
        [ForeignKey("Active_IngredientID")]
        public int Active_IngredientID { get; set; }

        public Active_Ingredient Active_Ingredients { get; set; }
        public Medication Medications { get; set; }

        [Required]
        public string Strength { get; set; }
    }
}
