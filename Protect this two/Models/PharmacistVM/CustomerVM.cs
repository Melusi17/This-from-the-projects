using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.Design;

namespace IbhayiPharmacy.Models.PharmacistVM
{
    public class CustomerVM
    {
        public Active_Ingredient Active_Ingredient { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> AlergyList { get; set; }
    }
}
