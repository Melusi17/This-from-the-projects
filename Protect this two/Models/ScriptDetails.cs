using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models
{
    public class ScriptDetails
    {
        public int ScriptDetailsId { get; set; }

        public int ScriptHeaderId { get; set; }
        [ForeignKey("ScriptHeaderId")]
        [ValidateNever]
        public ScriptHeader ScriptHeader { get; set; }



        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Prescription Prescription { get; set; }

    }
}
