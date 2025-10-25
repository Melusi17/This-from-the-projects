using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IbhayiPharmacy.Models.PharmacistVM
{
    public class CustomerScriptsVM
    {
        public int Prescr { get; set; }

        [Display(Name = "First Name")]
        public string? Name { get; set; }

        [Display(Name = "Last Name")]
        public string? Surname { get; set; }

        [Display(Name = "ID Number")]
        public string? IDNumber { get; set; }

        public List<Prescription> ScriptList { get; set; } = new List<Prescription>();

        [Display(Name = "Prescription Date")]
        [DataType(DataType.Date)]
        public DateTime PrescriptionDate { get; set; }

        [Display(Name = "Script Lines")]
        public List<ScriptLineVM> ScriptLines { get; set; } = new List<ScriptLineVM>();

        // NEW: Doctor at prescription level (not per medication)
        [Required(ErrorMessage = "Please select a doctor for the prescription")]
        [Display(Name = "Prescribing Doctor")]
        public int? DoctorId { get; set; }

        [Display(Name = "Doctor")]
        public string? DoctorName { get; set; }

        // Helper properties
        public bool HasAllergyConflicts { get; set; }
        public List<string> CustomerAllergies { get; set; } = new List<string>();

        // UI helper for doctor validation
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasDoctor => DoctorId.HasValue && DoctorId > 0;
    }

    public class ScriptLineVM
    {
        public int ScriptLineId { get; set; }

        [Required(ErrorMessage = "Please select a medication")]
        [Display(Name = "Medication")]
        public int MedicationId { get; set; }

        [Display(Name = "Medication Name")]
        public string? MedicationName { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Instructions are required")]
        [StringLength(500, ErrorMessage = "Instructions cannot exceed 500 characters")]
        [Display(Name = "Instructions")]
        public string Instructions { get; set; } = string.Empty;

        [Display(Name = "Is Repeat")]
        public bool IsRepeat { get; set; }

        [Display(Name = "Repeats Left")]
        [Range(0, 12, ErrorMessage = "Repeats must be between 0 and 12")]
        public int RepeatsLeft { get; set; }

        // REMOVED: DoctorId and DoctorName from here - now at prescription level

        [Required(ErrorMessage = "Please select a status")]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Rejection Reason")]
        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        public string? RejectionReason { get; set; }

        // UI helper properties (not for data binding)
        [System.Text.Json.Serialization.JsonIgnore]
        public bool ShowRejectionReason => Status == "Rejected";

        // CHANGED: Now checks if parent prescription has a doctor
        [System.Text.Json.Serialization.JsonIgnore]
        public bool CanBeApproved { get; set; } = false;

        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasStock { get; set; } = true;

        [System.Text.Json.Serialization.JsonIgnore]
        public int AvailableStock { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public List<string> AllergyConflicts { get; set; } = new List<string>();

        // UPDATED Validation method - no more doctor validation here
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Doctor validation removed - now handled at prescription level

            if (Status == "Rejected" && string.IsNullOrWhiteSpace(RejectionReason))
            {
                yield return new ValidationResult(
                    "Rejection reason is required for rejected medications",
                    new[] { nameof(RejectionReason) });
            }
        }
    }

    public class ScriptLineStatusUpdate
    {
        [Required]
        public int ScriptLineId { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        public string? RejectionReason { get; set; }

        // REMOVED: DoctorId - no longer needed per script line
    }
}