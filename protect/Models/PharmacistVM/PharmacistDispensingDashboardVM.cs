using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IbhayiPharmacy.Models.PharmacistVM
{
    public class PharmacistDispensingDashboardVM
    {
        public List<Order> PendingOrders { get; set; } = new List<Order>();
        public List<Order> ReadyForCollection { get; set; } = new List<Order>();
        public List<Order> WaitingCustomerAction { get; set; } = new List<Order>();
        public int TodayDispensed { get; set; }

        // Calculated properties
        public int TotalOrders => PendingOrders.Count + ReadyForCollection.Count + WaitingCustomerAction.Count;
        public bool HasPendingOrders => PendingOrders.Count > 0;
        public bool HasReadyOrders => ReadyForCollection.Count > 0;
        public bool HasWaitingOrders => WaitingCustomerAction.Count > 0;
    }

    public class DispenseOrderVM
    {
        public int OrderID { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerIDNumber { get; set; } = string.Empty;
        public List<string> CustomerAllergies { get; set; } = new List<string>();
        public string CurrentStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int VAT { get; set; }
        public List<DispenseOrderLineVM> OrderLines { get; set; } = new List<DispenseOrderLineVM>();

        // Processing state properties
        public bool AllItemsProcessed { get; set; }
        public bool AnyItemsDispensed { get; set; }
        public bool AllItemsRejected { get; set; }

        // For selected medications to dispense
        public List<int> SelectedOrderLineIds { get; set; } = new List<int>();

        // NEW CALCULATED PROPERTIES FOR UI
        public int PendingCount => OrderLines.Count(ol => ol.Status == "Pending");
        public int DispensedCount => OrderLines.Count(ol => ol.Status == "Dispensed");
        public int RejectedCount => OrderLines.Count(ol => ol.Status == "Rejected");
        public bool CanCompleteOrder => PendingCount == 0;
        public string ExpectedOrderStatus => AnyItemsDispensed ? "Ready for Collection" : "Waiting Customer Action";
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

        // NEW REPEATS PROPERTIES
        public int TotalRepeats { get; set; }
        public int RepeatsLeft { get; set; }

        // Selection properties
        public bool IsSelected { get; set; }
        public bool CanBeSelected => Status == "Pending";

        // UI helper properties
        public bool HasAllergyConflict { get; set; }
        public string AllergyWarning { get; set; } = string.Empty;
        public string StockStatus => IsLowStock ? "Low Stock" : "In Stock";
        public string StockStatusClass => IsLowStock ? "text-danger" : "text-success";

        // NEW REPEATS UI PROPERTIES
        public bool HasRepeats => TotalRepeats > 0;
        public string RepeatsDisplay => HasRepeats ? $"{RepeatsLeft}/{TotalRepeats}" : "0";
        public string RepeatsTooltip => HasRepeats ? $"Repeats: {RepeatsLeft} left of {TotalRepeats} total" : "No repeats available";
    }

    public class DispensingHistoryVM
    {
        public List<Order> Orders { get; set; } = new List<Order>();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalProcessed { get; set; }
        public int ReadyForCollectionCount { get; set; }
        public int WaitingActionCount { get; set; }
        public decimal TotalRevenue { get; set; }

        // Filter properties
        public string StatusFilter { get; set; } = "All";
        public string PatientFilter { get; set; } = string.Empty;

        // Calculated properties
        public decimal AverageOrderValue => TotalProcessed > 0 ? TotalRevenue / TotalProcessed : 0;
        public string RevenueFormatted => TotalRevenue.ToString("C");
        public string AverageOrderValueFormatted => AverageOrderValue.ToString("C");
    }

    public class RejectOrderLineVM
    {
        public int OrderLineID { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rejection reason is required")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string RejectionReason { get; set; } = string.Empty;

        // Predefined rejection reasons
        public List<string> CommonReasons => new List<string>
        {
            "Out of stock",
            "Patient allergic to ingredient",
            "Patient already has sufficient supply",
            "Prescription expired",
            "Dosage form not available",
            "Insurance not covering",
            "Other"
        };
    }

    public class CompleteOrderProcessingVM
    {
        public int OrderID { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int DispensedCount { get; set; }
        public int RejectedCount { get; set; }
        public int PendingCount { get; set; }
        public bool CanComplete { get; set; }
        public string ExpectedStatus { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;

        // Confirmation messages
        public string SuccessMessage => $"Order will be marked as {ExpectedStatus}";
        public string WarningMessage => PendingCount > 0 ? $"{PendingCount} medications still pending" : "";
    }
}