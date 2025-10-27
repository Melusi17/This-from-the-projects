using System;
using System.Collections.Generic;

namespace IbhayiPharmacy.Models.PharmacistVM
{
    public class RefillMedicationVM
    {
        public int ScriptLineID { get; set; }
        public int PrescriptionID { get; set; }
        public int MedicationID { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public int TotalRepeats { get; set; }
        public int RepeatsLeft { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime LastRefillDate { get; set; }
        public int CurrentPrice { get; set; }
        public string Schedule { get; set; } = string.Empty;

        // Added for prescription grouping
        public DateTime PrescriptionDate { get; set; }
    }

    public class PrescriptionRefillVM
    {
        public int PrescriptionID { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime PrescriptionDate { get; set; }
        public List<RefillMedicationVM> Medications { get; set; } = new();

        // Helper property for display
        public string FormattedDate => PrescriptionDate.ToString("yyyy-MM-dd");

        // Helper property to check if any medications have repeats
        public bool HasAvailableRefills => Medications.Any(m => m.RepeatsLeft > 0);
    }
}