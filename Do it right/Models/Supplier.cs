using System.ComponentModel.DataAnnotations;

namespace IbhayiPharmacy.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }

        [Required]
        public string SupplierName { get; set; }

        public String ContactName { get; set; }

        public string ContactSurname { get; set; }

        //[Required]
        //public string ContactNumber { get; set; }

        [Required]
        public string EmailAddress { get; set; }
    }
}
