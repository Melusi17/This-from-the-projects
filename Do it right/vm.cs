namespace IbhayiPharmacy.Models.PharmacistVM
{
    // View Models
    public class CustomerDashboardVM
    {
        public List<Prescription> UnprocessedPrescriptions { get; set; } = new();
        public List<Prescription> ProcessedPrescriptions { get; set; } = new();
    }

    public class PlaceOrderVM
    {
        public List<Doctor> Doctors { get; set; } = new();
        public List<Medication> Medications { get; set; } = new();
        public DateTime OrderDate { get; set; }
    }

    public class RepeatMedicationVM
    {
        public int ScriptLineId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public int RepeatsLeft { get; set; }
        public DateTime LastRefillDate { get; set; }
        public string Instructions { get; set; } = string.Empty;
    }

    public class OrderSubmissionVM
    {
        public List<OrderItemVM> OrderItems { get; set; } = new();
        public int? DoctorId { get; set; }
    }

    public class OrderItemVM
    {
        public int MedicationId { get; set; }
        public int Quantity { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public bool IsRepeat { get; set; }
    }

    public class ReportRequestVM
    {
        public DateTime ReportDate { get; set; }
    }


}
