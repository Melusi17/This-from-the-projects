using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IbhayiPharmacy.Models.CustomerVM
{
    public class CustomerRegisterViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Surname is required")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "ID Number is required")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "ID must be 13 digits")]
        public string IdNumber { get; set; }

        [Required(ErrorMessage = "Cellphone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Cellphone must be 10 digits")]
        public string Cellphone { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        // Allergies
        // Hidden field for storing selected allergy IDs as CSV
        [ValidateNever]
        public string SelectedAllergies { get; set; }
        [ValidateNever]

        // Dropdown source
        public List<SelectListItem> AllergiesList { get; set; }

    }
}
