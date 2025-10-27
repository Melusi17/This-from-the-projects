using System.ComponentModel.DataAnnotations;

namespace IbhayiPharmacy.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        public string HealthCouncilRegistrationNumber { get; set; }

        [Required]
        public string ContactNumber { get; set; }

        [Required]
        public string Email { get; set; }
    }
}
