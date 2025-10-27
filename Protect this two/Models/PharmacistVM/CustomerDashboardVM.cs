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
        public int ScriptLineId { get; set; }
        public int Quantity { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public bool IsRepeat { get; set; }
    }

    public class ReportRequestVM
    {
        public DateTime ReportDate { get; set; }
    }

    // NEW: Order Tracking View Model
    public class OrderTrackingVM
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TotalDue { get; set; } = string.Empty;
        public List<OrderLineVM> OrderLines { get; set; } = new();
    }

    // NEW: Order Line View Model for tracking
    public class OrderLineVM
    {
        public string MedicationName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public int ItemPrice { get; set; }
        public string Status { get; set; } = "Pending";
        public string? RejectionReason { get; set; }
        public string DoctorName { get; set; } = string.Empty; // For the table display
    }
}