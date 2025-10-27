using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models
{
    public class Pharmacist
    {
        [Key]
        public int PharmacistID { get; set; }



        [ForeignKey("ApplicationUserId")]
        public string ApplicationUserId { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public string HealthCouncilRegNo { get; set; }
        
        
    }
}
