namespace IbhayiPharmacy.Models.PharmacistVM
{
    public class CustomerOrderVM
    {
        public bool IsSelected { get; set; }
        public int ScriptLineID { get; set; }
        public int PrescriptionID { get; set; }
        public int Quantity { get; set; }
        public int TotalRepeats { get; set; }
        public int RepeatsLeft { get; set; }
        public string Instructions { get; set; } = string.Empty; // Add this
        public int MedicationID { get; set; }
        public string MedicationName { get; set; } = string.Empty; // Add this
        public decimal CurrentPrice { get; set; }
        public string Schedule { get; set; } = string.Empty; // Add this
        public string DoctorName { get; set; } = string.Empty; // Add this
        public string DoctorSurname { get; set; } = string.Empty; // Add this
        public string FullDoctorName => $"Dr. {DoctorName} {DoctorSurname}";
        public decimal LineTotal => Quantity * CurrentPrice;
        public string RepeatsDisplay => $"{RepeatsLeft} of {TotalRepeats}";
    }

    public class OrderSummaryVM
    {
        public List<CustomerOrderVM> SelectedItems { get; set; } = new List<CustomerOrderVM>();
        public decimal Subtotal => SelectedItems.Sum(item => item.LineTotal);
        public decimal Tax => Subtotal * 0.15m;
        public decimal Total => Subtotal + Tax;
        public string OrderNumber { get; set; } = string.Empty; // Add this
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string CustomerName { get; set; } = string.Empty; // Add this
        public string CustomerEmail { get; set; } = string.Empty; // Add this
    }

    public class OrderConfirmationVM
    {
        public int OrderID { get; set; }
        public string OrderNumber { get; set; } = string.Empty; // Add this
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty; // Add this
        public decimal TotalAmount { get; set; }
        public List<OrderLineDetailVM> OrderLines { get; set; } = new List<OrderLineDetailVM>();
        public string CustomerName { get; set; } = string.Empty; // Add this
        public string CustomerEmail { get; set; } = string.Empty; // Add this
    }

    public class OrderLineDetailVM
    {
        public string MedicationName { get; set; } = string.Empty; // Add this
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string DoctorName { get; set; } = string.Empty; // Add this
        public string Instructions { get; set; } = string.Empty; // Add this
        public decimal LineTotal => Quantity * Price;
    }

    public class PlaceOrderFormVM
    {
        public List<int> SelectedScriptLineIds { get; set; } = new List<int>();
    }
}