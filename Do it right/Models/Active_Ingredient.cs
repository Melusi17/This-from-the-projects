using System.ComponentModel.DataAnnotations;

namespace IbhayiPharmacy.Models
{
    public class Active_Ingredient
    {
        [Key]
        public int Active_IngredientID { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
