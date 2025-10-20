using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace IbhayiPharmacy.Models
{
    public class NewScript
    {
        [Key]
        public int PrescriptionID { get; set; }

        //[ForeignKey("ApplicationUserId")]
        //public string ApplicationUserId { get; set; }
        //[ValidateNever]
        //public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public DateTime DateIssued { get; set; }
        [ValidateNever]
        public byte[] Script { get; set; }

        public bool DispenseUponApproval { get; set; }

        public string? Status { get; set; } = "Unprocessed";
        public List<PresScriptLine>? scriptLines { get; set; } = new List<PresScriptLine>();


    }
}
