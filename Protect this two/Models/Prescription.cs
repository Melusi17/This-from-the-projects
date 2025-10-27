using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace IbhayiPharmacy.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionID { get; set; }

        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public string ApplicationUserId { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public DateTime DateIssued { get; set; }=System.DateTime.Now;
        [ValidateNever]
        public byte[] Script { get; set; }
        [ForeignKey("DoctorID")]
        public int? DoctorID { get; set; }
        [ValidateNever]
        public Doctor Doctors { get; set; }

        public bool DispenseUponApproval { get; set; }
        public string? Status { get; set; } = "Unprocessed";
        public List<ScriptLine>? scriptLines { get; set; } = new List<ScriptLine>();


    }
}
