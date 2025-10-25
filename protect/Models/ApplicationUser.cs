using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models
{
    public class ApplicationUser : IdentityUser
    {
       

        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }
        [Required]
        public string IDNumber { get; set; }

        public string? CellphoneNumber { get; set; }
        public int? Active_IngredientID { get; set; }

        [NotMapped]
        public string? Role { get; internal set; }
        [NotMapped]
        public string RoleId { get; set; }
        [NotMapped]
        public string UserClaim { get; set; }

    }
}
