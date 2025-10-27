using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models.PharmacistVM
{
    public class WalkInPrescriptionVM
    {
        // Patient Info
        [Display(Name = "Patient")]
        [Required(ErrorMessage = "Please select a patient")]
        public int? CustomerId { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerIDNumber { get; set; }

        // Doctor Info  
        [Display(Name = "Doctor")]
        [Required(ErrorMessage = "Please select a doctor")]
        public int? DoctorId { get; set; }

        public string? DoctorName { get; set; }

        // Prescription Info
        [Required(ErrorMessage = "Prescription date is required")]
        [Display(Name = "Prescription Date")]
        [DataType(DataType.Date)]
        public DateTime PrescriptionDate { get; set; } = DateTime.Now;

        [Display(Name = "Dispense Upon Approval")]
        public bool DispenseUponApproval { get; set; } = true;

        [Display(Name = "Upload Prescription (Optional)")]
        public IFormFile? PrescriptionFile { get; set; }

        // Medications
        public List<WalkInScriptLineVM> ScriptLines { get; set; } = new() { new WalkInScriptLineVM() };
    }

    public class WalkInScriptLineVM
    {
        [Required(ErrorMessage = "Medication is required")]
        [Display(Name = "Medication")]
        public int MedicationId { get; set; }

        public string MedicationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Dosage form is required")]
        [Display(Name = "Dosage Form")]
        public int DosageFormId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "Instructions are required")]
        [StringLength(500, ErrorMessage = "Instructions too long")]
        public string Instructions { get; set; } = "Take as directed";

        [Display(Name = "Repeat Prescription")]
        public bool IsRepeat { get; set; }

        [Display(Name = "Repeats Left")]
        [Range(0, 12, ErrorMessage = "Repeats must be between 0 and 12")]
        public int RepeatsLeft { get; set; }

        // NEW: Status and Rejection fields (from ScriptsProcessedController)
        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Approved"; // Default to Approved for walk-ins

        [Display(Name = "Rejection Reason")]
        [StringLength(500, ErrorMessage = "Rejection reason too long")]
        public string? RejectionReason { get; set; }
    }

    // ========== AJAX REQUEST MODELS ==========

    public class SearchCustomersRequest
    {
        [Required(ErrorMessage = "Search term is required")]
        [StringLength(50, ErrorMessage = "Search term too long")]
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class SearchDoctorsRequest
    {
        [Required(ErrorMessage = "Search term is required")]
        [StringLength(50, ErrorMessage = "Search term too long")]
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class SearchMedicationsRequest
    {
        [Required(ErrorMessage = "Search term is required")]
        [StringLength(50, ErrorMessage = "Search term too long")]
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class RegisterPatientRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, ErrorMessage = "Name too long")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Surname is required")]
        [StringLength(50, ErrorMessage = "Surname too long")]
        public string Surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID Number is required")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "ID Number must be 13 digits")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "ID Number must contain only digits")]
        public string IDNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cellphone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Cellphone { get; set; } = string.Empty;

        // CHANGED: Use Active Ingredient IDs instead of string names
        public List<int> SelectedAllergyIds { get; set; } = new List<int>();

        // Password fields for user registration
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class RegisterDoctorRequest
    {
        [Required(ErrorMessage = "Doctor name is required")]
        [StringLength(50, ErrorMessage = "Name too long")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doctor surname is required")]
        [StringLength(50, ErrorMessage = "Surname too long")]
        public string Surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Practice number is required")]
        [StringLength(20, ErrorMessage = "Practice number too long")]
        [Display(Name = "Practice Number")]
        public string PracticeNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;
    }

    public class CheckAllergyRequest
    {
        [Required(ErrorMessage = "Customer ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid customer ID")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Medication ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid medication ID")]
        public int MedicationId { get; set; }
    }

    public class AddMedicationRequest
    {
        [Required(ErrorMessage = "Medication ID is required")]
        public int MedicationId { get; set; }

        [Required(ErrorMessage = "Dosage form ID is required")]
        public int DosageFormId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; } = 1;
    }

    // ========== AJAX RESPONSE MODELS ==========

    public class SearchResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public object? Data { get; set; }
    }

    public class CustomerSearchResult
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string IDNumber { get; set; } = string.Empty;
        // CHANGED: Return allergy names for display
        public List<string> AllergyNames { get; set; } = new();
    }

    public class DoctorSearchResult
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string PracticeNumber { get; set; } = string.Empty;
    }

    public class MedicationSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DosageForm { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int ReorderLevel { get; set; }
        public List<string> ActiveIngredients { get; set; } = new();
        public bool IsLowStock => Stock <= ReorderLevel + 10;
    }

    // NEW: Active Ingredient model for allergy selection
    public class ActiveIngredientResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AllergyCheckResult
    {
        public bool HasConflicts { get; set; }
        public List<string> Conflicts { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class RegistrationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? Id { get; set; }
        public string? Error { get; set; }
    }

    // NEW: Customer allergy result
    public class CustomerAllergyResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}