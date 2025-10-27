using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models
{
    public class Custormer_Allergy
    {
        [Key]
        public int Custormer_AllergyID { get; set; }

        [ForeignKey("Customer")]
        public int CustomerID { get; set; }
        [ValidateNever]
        public Customer Customer { get; set; }

        [ForeignKey("Active_Ingredient")]
        public int Active_IngredientID { get; set; }
        [ValidateNever]
        public Active_Ingredient Active_Ingredient { get; set; }
        
    }
}
