using IbhayiPharmacy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace IbhayiPharmacy.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser> 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<PresScriptLine> PresScriptLines { get; set; }
        public DbSet<Pharmacist> Pharmacists { get; set; }
        public DbSet<NewScript> NewScripts { get; set; }
        public DbSet<Pharmacy> Pharmacies { get; set; }
        public DbSet<PharmacyManager> PharmacyManagers { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Custormer_Allergy> Custormer_Allergies { get; set; }
        public DbSet<DosageForm> DosageForms { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Active_Ingredient> Active_Ingredients { get; set; }
        public DbSet<Medication_Ingredient> Medication_Ingredients { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<UnprocessedScripts> UnprocessedScripts { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<StockOrder> StockOrders { get; set; }
        public DbSet<StockOrderDetail> StockOrderDetails { get; set; }
        public DbSet<ScriptLine> ScriptLines { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            




            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Active Ingredients
            modelBuilder.Entity<Active_Ingredient>().HasData(
                new Active_Ingredient { Active_IngredientID = 1, Name = "Pylorazine" },
                new Active_Ingredient { Active_IngredientID = 2, Name = "Vaspril" },
                new Active_Ingredient { Active_IngredientID = 3, Name = "Zentropine" },
                new Active_Ingredient { Active_IngredientID = 4, Name = "Histarelin" },
                new Active_Ingredient { Active_IngredientID = 5, Name = "Lorvexamine" },
                new Active_Ingredient { Active_IngredientID = 6, Name = "Aterolazine" },
                new Active_Ingredient { Active_IngredientID = 7, Name = "Bronchomid" },
                new Active_Ingredient { Active_IngredientID = 8, Name = "Alveclear" },
                new Active_Ingredient { Active_IngredientID = 9, Name = "Epidraxol" },
                new Active_Ingredient { Active_IngredientID = 10, Name = "Cortizane" },
                new Active_Ingredient { Active_IngredientID = 11, Name = "Glycerrol" },
                new Active_Ingredient { Active_IngredientID = 12, Name = "Sonnexil" },
                new Active_Ingredient { Active_IngredientID = 13, Name = "Calcitrine" },
                new Active_Ingredient { Active_IngredientID = 14, Name = "Phospholax" },
                new Active_Ingredient { Active_IngredientID = 15, Name = "Virocelin" },
                new Active_Ingredient { Active_IngredientID = 16, Name = "Immubrine" },
                new Active_Ingredient { Active_IngredientID = 17, Name = "Trosamine" },
                new Active_Ingredient { Active_IngredientID = 18, Name = "Velocidine" },
                new Active_Ingredient { Active_IngredientID = 19, Name = "Nexorin" },
                new Active_Ingredient { Active_IngredientID = 20, Name = "Zyphralex" },
                new Active_Ingredient { Active_IngredientID = 21, Name = "Cardionol" },
                new Active_Ingredient { Active_IngredientID = 22, Name = "Alveretol" },
                new Active_Ingredient { Active_IngredientID = 23, Name = "Xylogran" },
                new Active_Ingredient { Active_IngredientID = 24, Name = "Fematrix" },
                new Active_Ingredient { Active_IngredientID = 25, Name = "Plastorin" },
                new Active_Ingredient { Active_IngredientID = 26, Name = "Seralox" },
                new Active_Ingredient { Active_IngredientID = 27, Name = "Quantrel" },
                new Active_Ingredient { Active_IngredientID = 28, Name = "Myvetrin" },
                new Active_Ingredient { Active_IngredientID = 29, Name = "Draxolene" },
                new Active_Ingredient { Active_IngredientID = 30, Name = "Veltraxin" }
            );

            // Seed Dosage Forms
            modelBuilder.Entity<DosageForm>().HasData(
                new DosageForm { DosageFormID = 1, DosageFormName = "Tablet" },
                new DosageForm { DosageFormID = 2, DosageFormName = "Capsule" },
                new DosageForm { DosageFormID = 3, DosageFormName = "Suspension" },
                new DosageForm { DosageFormID = 4, DosageFormName = "Syrup" },
                new DosageForm { DosageFormID = 5, DosageFormName = "Lotion" },
                new DosageForm { DosageFormID = 6, DosageFormName = "Spray" },
                new DosageForm { DosageFormID = 7, DosageFormName = "Gel" },
                new DosageForm { DosageFormID = 8, DosageFormName = "Suppository" },
                new DosageForm { DosageFormID = 9, DosageFormName = "Injectable" },
                new DosageForm { DosageFormID = 10, DosageFormName = "Drops" },
                new DosageForm { DosageFormID = 11, DosageFormName = "IV Drip" },
                new DosageForm { DosageFormID = 12, DosageFormName = "Powder" }
            );




            // Seed Suppliers
            modelBuilder.Entity<Supplier>().HasData(
                new Supplier { SupplierID = 1, SupplierName = "NovaCure", ContactName = "Davie", ContactSurname = "Jones", EmailAddress = "davie@example.com" },
                new Supplier { SupplierID = 2, SupplierName = "HelixMed", ContactName = "Nicky", ContactSurname = "Mostert", EmailAddress = "nmostert@mandela.ac.za" },
                new Supplier { SupplierID = 3, SupplierName = "VitaGenix", ContactName = "Matimu", ContactSurname = "Vuqa", EmailAddress = "matimu@example.com" },
                new Supplier { SupplierID = 4, SupplierName = "Apex Biomed", ContactName = "Lulu", ContactSurname = "Ndhambi", EmailAddress = "lulu@example.com" },
                new Supplier { SupplierID = 5, SupplierName = "CuraNova", ContactName = "Pharmacy Manager Group Member Name", ContactSurname = "Pharmacy Manager Group Member Surname", EmailAddress = "Pharmacy Manager Group Member E-mail" }
            );

            // Seed Doctors
            modelBuilder.Entity<Doctor>().HasData(
                new Doctor { DoctorID = 1, Name = "Charmaine", Surname = "Meintjies", ContactNumber = "071 234 5678", Email = "charmaine@example.com", HealthCouncilRegistrationNumber = "976431" },
                new Doctor { DoctorID = 2, Name = "Jacob", Surname = "Moloi", ContactNumber = "072 234 5678", Email = "jacob@example.com", HealthCouncilRegistrationNumber = "316497" },
                new Doctor { DoctorID = 3, Name = "David", Surname = "Greeff", ContactNumber = "073 234 5678", Email = "david@gmail.example", HealthCouncilRegistrationNumber = "718293" },
                new Doctor { DoctorID = 4, Name = "Karien", Surname = "Momberg", ContactNumber = "075 234 5678", Email = "karien@example.com", HealthCouncilRegistrationNumber = "159753" },
                new Doctor { DoctorID = 5, Name = "Felicity", Surname = "Daniels", ContactNumber = "076 234 5678", Email = "felicity@example.com", HealthCouncilRegistrationNumber = "951357" },
                new Doctor { DoctorID = 6, Name = "Errol", Surname = "Pieterse", ContactNumber = "078 234 5678", Email = "errol@example.com", HealthCouncilRegistrationNumber = "852456" },
                new Doctor { DoctorID = 7, Name = "Alyce", Surname = "Morapedi", ContactNumber = "079 234 5678", Email = "alyce@example.com", HealthCouncilRegistrationNumber = "654852" }
            );

            // Seed Users
            //modelBuilder.Entity<ApplicationUser>().HasData(
            //    // Pharmacists
            //    new ApplicationUser { UserId = 1, Name = "Lindile", Surname = "Hadebe", IDNumber = 123456, CellphoneNumber = 612345678, Email = "lindile@example.com", Password = "password1", Role = "Pharmacist" },
            //    new ApplicationUser { UserId = 2, Name = "Lindile Dorothy", Surname = "Daniels", IDNumber = 234567, CellphoneNumber = 622345678, Email = "dorothy@example.com", Password = "password2", Role = "Pharmacist" },
            //    new ApplicationUser { UserId = 3, Name = "Marcel", Surname = "Van Niekerk", IDNumber = 345678, CellphoneNumber = 632345678, Email = "marcel@example.com", Password = "password3", Role = "Pharmacist" },
            //    new ApplicationUser { UserId = 4, Name = "Nicky", Surname = "Mostert", IDNumber = 190406, CellphoneNumber = 721234567, Email = "nicky.mostert@mandela.ac.za", Password = "password4", Role = "Pharmacist" },

            //    // Pharmacy Manager
            //    new ApplicationUser { UserId = 5, Name = "Pharmacy Manager Group Member Name", Surname = "Pharmacy Manager Group Member Surname", IDNumber = 134679, CellphoneNumber = 123456789, Email = "Pharmacy Manager Group Member E-mail", Password = "managerpass", Role = "PharmacyManager" },

            //    // Sample Customers
            //    new ApplicationUser { UserId = 6, Name = "John", Surname = "Doe", IDNumber = 500101, CellphoneNumber = 831234567, Email = "john@example.com", Password = "customer1", Role = "Customer" },
            //    new ApplicationUser { UserId = 7, Name = "Jane", Surname = "Smith", IDNumber = 600202, CellphoneNumber = 841234567, Email = "jane@example.com", Password = "customer2", Role = "Customer" },
            //    new ApplicationUser { UserId = 8, Name = "Bob", Surname = "Johnson", IDNumber = 700303, CellphoneNumber = 851234567, Email = "bob@example.com", Password = "customer3", Role = "Customer" }
            //);

            //// Seed Pharmacy Manager
            //modelBuilder.Entity<PharmacyManager>().HasData(
            //    new PharmacyManager { PharmacyManagerID = 1, UserId = 5, HealthCouncilRegNo = "134679" }
            //);

            //// Seed Pharmacists
            //modelBuilder.Entity<Pharmacist>().HasData(
            //    new Pharmacist { PharmacistID = 1, UserId = 1, HealthCouncilRegNo = "123456" },
            //    new Pharmacist { PharmacistID = 2, UserId = 2, HealthCouncilRegNo = "234567" },
            //    new Pharmacist { PharmacistID = 3, UserId = 3, HealthCouncilRegNo = "345678" },
            //    new Pharmacist { PharmacistID = 4, UserId = 4, HealthCouncilRegNo = "190406" }
            //);


            //// Seed Customers
            //modelBuilder.Entity<Customer>().HasData(
            //    new Customer { CustormerID = 1, UserId = 6, Allergy = "None" },
            //    new Customer { CustormerID = 2, UserId = 7, Allergy = "Pollen" },
            //    new Customer { CustormerID = 3, UserId = 8, Allergy = "Dust" }
            //);

            //// Seed Customer Allergies
            //modelBuilder.Entity<CustomerAllergy>().HasData(
            //    new CustomerAllergy { Customer_AllergyId = 1, CustomerId = 2, Active_IngredientID = 4 }, // Jane allergic to Histarelin
            //    new CustomerAllergy { Customer_AllergyId = 2, CustomerId = 3, Active_IngredientID = 7 }  // Bob allergic to Bronchomid
            //);

            // Seed Medications
            modelBuilder.Entity<Medication>().HasData(
                new Medication { MedcationID = 1, MedicationName = "CardioVex", Schedule = "6", CurrentPrice = 150, DosageFormID = 1, SupplierID = 1, ReOrderLevel = 100, QuantityOnHand = 90 },
                new Medication { MedcationID = 2, MedicationName = "Neurocalm", Schedule = "2", CurrentPrice = 200, DosageFormID = 1, SupplierID = 2, ReOrderLevel = 110, QuantityOnHand = 100 },
                new Medication { MedcationID = 3, MedicationName = "Allerfree Duo", Schedule = "0", CurrentPrice = 180, DosageFormID = 12, SupplierID = 3, ReOrderLevel = 150, QuantityOnHand = 100 },
                new Medication { MedcationID = 4, MedicationName = "GastroEase", Schedule = "3", CurrentPrice = 95, DosageFormID = 1, SupplierID = 4, ReOrderLevel = 400, QuantityOnHand = 470 },
                new Medication { MedcationID = 5, MedicationName = "Respivent", Schedule = "3", CurrentPrice = 120, DosageFormID = 1, SupplierID = 5, ReOrderLevel = 300, QuantityOnHand = 490 },
                new Medication { MedcationID = 6, MedicationName = "Dermagard", Schedule = "3", CurrentPrice = 85, DosageFormID = 1, SupplierID = 2, ReOrderLevel = 600, QuantityOnHand = 790 },
                new Medication { MedcationID = 7, MedicationName = "Metaborex", Schedule = "4", CurrentPrice = 210, DosageFormID = 1, SupplierID = 2, ReOrderLevel = 200, QuantityOnHand = 250 },
                new Medication { MedcationID = 8, MedicationName = "Sleeptraze", Schedule = "2", CurrentPrice = 175, DosageFormID = 1, SupplierID = 2, ReOrderLevel = 100, QuantityOnHand = 110 },
                new Medication { MedcationID = 9, MedicationName = "OsteoFlex", Schedule = "3", CurrentPrice = 300, DosageFormID = 3, SupplierID = 2, ReOrderLevel = 200, QuantityOnHand = 210 },
                new Medication { MedcationID = 10, MedicationName = "Immunexin", Schedule = "6", CurrentPrice = 450, DosageFormID = 9, SupplierID = 2, ReOrderLevel = 200, QuantityOnHand = 190 },
                new Medication { MedcationID = 11, MedicationName = "CardioPlus", Schedule = "6", CurrentPrice = 600, DosageFormID = 11, SupplierID = 2, ReOrderLevel = 500, QuantityOnHand = 600 },
                new Medication { MedcationID = 12, MedicationName = "AllerCalm", Schedule = "6", CurrentPrice = 350, DosageFormID = 11, SupplierID = 2, ReOrderLevel = 400, QuantityOnHand = 410 },
                new Medication { MedcationID = 13, MedicationName = "RespirAid", Schedule = "6", CurrentPrice = 280, DosageFormID = 9, SupplierID = 2, ReOrderLevel = 100, QuantityOnHand = 100 },
                new Medication { MedcationID = 14, MedicationName = "DermaClear", Schedule = "6", CurrentPrice = 125, DosageFormID = 5, SupplierID = 2, ReOrderLevel = 100, QuantityOnHand = 200 },
                new Medication { MedcationID = 15, MedicationName = "OsteoPrime", Schedule = "6", CurrentPrice = 190, DosageFormID = 2, SupplierID = 2, ReOrderLevel = 100, QuantityOnHand = 400 }
            );

            // Seed Medication Ingredients
            modelBuilder.Entity<Medication_Ingredient>().HasData(
                new Medication_Ingredient { Medication_IngredientID = 1, MedicationID = 1, Active_IngredientID = 6, Strength = "18mg" },
                new Medication_Ingredient { Medication_IngredientID = 2, MedicationID = 2, Active_IngredientID = 2, Strength = "2mg" },
                new Medication_Ingredient { Medication_IngredientID = 3, MedicationID = 2, Active_IngredientID = 3, Strength = "50mg" },
                new Medication_Ingredient { Medication_IngredientID = 4, MedicationID = 3, Active_IngredientID = 4, Strength = "325mg" },
                new Medication_Ingredient { Medication_IngredientID = 5, MedicationID = 3, Active_IngredientID = 5, Strength = "453.6g" },
                new Medication_Ingredient { Medication_IngredientID = 6, MedicationID = 4, Active_IngredientID = 1, Strength = "Standard" },
                new Medication_Ingredient { Medication_IngredientID = 7, MedicationID = 5, Active_IngredientID = 7, Strength = "Standard" },
                new Medication_Ingredient { Medication_IngredientID = 8, MedicationID = 5, Active_IngredientID = 8, Strength = "Standard" },
                new Medication_Ingredient { Medication_IngredientID = 9, MedicationID = 6, Active_IngredientID = 9, Strength = "Standard" },
                new Medication_Ingredient { Medication_IngredientID = 10, MedicationID = 6, Active_IngredientID = 10, Strength = "Standard" },
                new Medication_Ingredient { Medication_IngredientID = 11, MedicationID = 7, Active_IngredientID = 11, Strength = "Standard" },
                new Medication_Ingredient { Medication_IngredientID = 12, MedicationID = 8, Active_IngredientID = 12, Strength = "Standard" },
                new Medication_Ingredient { Medication_IngredientID = 13, MedicationID = 9, Active_IngredientID = 13, Strength = "Standard" },
                new Medication_Ingredient { Medication_IngredientID = 14, MedicationID = 9, Active_IngredientID = 14, Strength = "Standard" },
                new Medication_Ingredient { Medication_IngredientID = 15, MedicationID = 10, Active_IngredientID = 15, Strength = "Standard" },
                new Medication_Ingredient { Medication_IngredientID = 16, MedicationID = 10, Active_IngredientID = 16, Strength = "Standard" },
                new Medication_Ingredient { Medication_IngredientID = 17, MedicationID = 11, Active_IngredientID = 13, Strength = "Standard" },
                new Medication_Ingredient { Medication_IngredientID = 18, MedicationID = 11, Active_IngredientID = 6, Strength = "Standard" },
                new Medication_Ingredient { Medication_IngredientID = 19, MedicationID = 12, Active_IngredientID = 4, Strength = "Standard" },
                new Medication_Ingredient { Medication_IngredientID = 20, MedicationID = 13, Active_IngredientID = 7, Strength = "Standard" },
                new Medication_Ingredient { Medication_IngredientID = 21, MedicationID = 14, Active_IngredientID = 9, Strength = "20mg" },
                new Medication_Ingredient { Medication_IngredientID = 22, MedicationID = 15, Active_IngredientID = 13, Strength = "20mg" }
            );

            
        }
    }
}