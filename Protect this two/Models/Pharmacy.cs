using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models
{
    public class Pharmacy
    {
        [Key]
        public int PharmacyID { get; set; }

        [ForeignKey("PharmacistID")]
        public int PharmacistID { get; set; }

        [ForeignKey("PharmacyManagerID")]
        public int PharmacyManagerID { get; set; }
        //public PharmacyManager PharmacyManager { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string HealthCouncilRegNo { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string WebsiteURL { get; set; }
    }
}
