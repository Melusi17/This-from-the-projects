using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IbhayiPharmacy.Models.PharmacistVM
{
    public class ProcessedPrescriptionVM
    {
        public int PrescriptionID { get; set; }

        [Display(Name = "Patient Name")]
        public string PatientName { get; set; } = string.Empty;

        [Display(Name = "ID Number")]
        public string IDNumber { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Date Issued")]
        [DataType(DataType.Date)]
        public DateTime DateIssued { get; set; }

        [Display(Name = "Doctor")]
        public string DoctorName { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Medications")]
        public List<ProcessedScriptLineVM> ScriptLines { get; set; } = new List<ProcessedScriptLineVM>();
    }

    public class ProcessedScriptLineVM
    {
        [Display(Name = "Medication")]
        public string MedicationName { get; set; } = string.Empty;

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Instructions")]
        public string Instructions { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Rejection Reason")]
        public string? RejectionReason { get; set; }

        [Display(Name = "Approved Date")]
        public DateTime? ApprovedDate { get; set; }

        [Display(Name = "Rejected Date")]
        public DateTime? RejectedDate { get; set; }
    }
}