using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models
{
    public class StockOrderDetail
    {
        [Key]
        public int StockOrderDetail_ID { get; set; }

        [ForeignKey("StockOrderID")]
        public int StockOrderID { get; set; }

        [ForeignKey("MedicationID")]
        public int MedicationID { get; set; }

        [Required]
        public int OrderQuantity { get; set; }
    }
}
