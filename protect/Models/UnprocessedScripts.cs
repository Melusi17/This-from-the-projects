using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IbhayiPharmacy.Models
{
    public class UnprocessedScripts
    {
        [Key]
        public int UnprocessedScriptID { get; set; }
        [ForeignKey("CustomerID")]
        public int CustomerID { get; set; }

        
        public DateTime Date { get; set; }

 
        public string Dispense { get; set; }


        public string Script { get; set; }
    }
}
