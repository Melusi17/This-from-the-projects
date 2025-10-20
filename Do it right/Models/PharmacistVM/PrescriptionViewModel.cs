using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IbhayiPharmacy.Models;

namespace IbhayiPharmacy.Models.PharmacistVM
{
    public class PrescriptionViewModel
    {
        public DateTime PrescriptionDate { get; set; } = DateTime.Now;

        // Doctors
        public List<Doctor> AvailableDoctors { get; set; } = new List<Doctor>();

        // Medication lines
        public List<ScriptLineViewModel> ScriptLines { get; set; } = new List<ScriptLineViewModel>();

        // Available medications
        public List<Medication> AvailableMedications { get; set; } = new List<Medication>();
    }


    public class ScriptLineViewModel
    {
        public int ScriptLineID { get; set; }
        [Required]
        public int MedicationID { get; set; }

        public Medication Medication { get; set; } = new Medication();

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public string Instructions { get; set; } = string.Empty;

        public int Repeats { get; set; }
        public int RepeatsLeft { get; set; }
    }

    public class NewScriptViewModel
    {
        public int PrescriptionID { get; set; }
        public byte[] Script { get; set; } = Array.Empty<byte>();
        public DateTime DateIssued { get; set; }

        public List<ScriptLine> ScriptLines { get; set; } = new List<ScriptLine>();
    }

    public class MedicationSelectionViewModel
    {
        public int MedcationID { get; set; }

        public string MedicationName { get; set; } = string.Empty;

        public string DosageForm { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;

        public List<Active_Ingredient> ActiveIngredients { get; set; } = new List<Active_Ingredient>();


        public int QuantityOnHand { get; set; }
        public int ReOrderLevel { get; set; }
        public decimal CurrentPrice { get; set; }
        public int SupplierID { get; set; }
        public Supplier Suppliers { get; set; } = new Supplier();
        public bool IsInStock => QuantityOnHand > 0;
        public bool IsLowStock => QuantityOnHand > 0 && QuantityOnHand < ReOrderLevel;
    }


}
