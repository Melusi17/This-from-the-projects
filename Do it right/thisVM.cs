// Add these to your PharmacistVM namespace

public class PharmacistDispensingDashboardVM
{
    public List<Order> PendingOrders { get; set; } = new();
    public List<Order> ReadyForCollection { get; set; } = new();
    public int TodayDispensed { get; set; }
}

public class DispenseOrderVM
{
    public int OrderID { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerIDNumber { get; set; } = string.Empty;
    public List<string> CustomerAllergies { get; set; } = new();
    public string CurrentStatus { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int VAT { get; set; }
    public List<DispenseOrderLineVM> OrderLines { get; set; } = new();
    public bool AllItemsReady { get; set; }
    public bool AnyItemsRejected { get; set; }
}

public class DispenseOrderLineVM
{
    public int OrderLineID { get; set; }
    public int MedicationID { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ItemPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public bool IsLowStock { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool CanDispense { get; set; }
    public string? RejectionReason { get; set; }
}

public class DispensingHistoryVM
{
    public List<Order> Orders { get; set; } = new();
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int TotalDispensed { get; set; }
    public decimal TotalRevenue { get; set; }
}