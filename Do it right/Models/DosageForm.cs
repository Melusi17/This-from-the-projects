using System.ComponentModel.DataAnnotations;

namespace IbhayiPharmacy.Models
{
    public class DosageForm
    {
        [Key]
        public int DosageFormID { get; set; }

        [Required]
        public string DosageFormName { get; set; }
    }
}
