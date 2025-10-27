using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models
{
    public class Customer
    {
        [Key]
        public int CustormerID { get; set; }

        [ForeignKey("ApplicationUserId")]
        public string ApplicationUserId { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        // Navigation property to the junction table
        [ValidateNever]
        public ICollection<Custormer_Allergy> CustomerAllergies { get; set; } = new List<Custormer_Allergy>();
    }
}
