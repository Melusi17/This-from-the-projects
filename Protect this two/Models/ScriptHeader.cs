using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models
{
    public class ScriptHeader
    {
        [Key]
        public int ScriptHeaderId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required]
        public string IDNumber { get; set; }
        public DateTime OrderDate { get; set; }=System.DateTime.Now;
        [Required]
        public string ScriptStatus { get; set; }

        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

    }
}
