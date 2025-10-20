using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IbhayiPharmacy.Migrations
{
    /// <inheritdoc />
    public partial class CHangedTables2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_ApplicationUserId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Custormer_Allergies_Active_Ingredients_Active_IngredientID",
                table: "Custormer_Allergies");

            migrationBuilder.DropForeignKey(
                name: "FK_Custormer_Allergies_Customers_CustomerID",
                table: "Custormer_Allergies");

            migrationBuilder.DropForeignKey(
                name: "FK_Medication_Ingredients_Active_Ingredients_Active_IngredientID",
                table: "Medication_Ingredients");

            migrationBuilder.DropForeignKey(
                name: "FK_Medication_Ingredients_Medications_MedicationID",
                table: "Medication_Ingredients");

            migrationBuilder.DropForeignKey(
                name: "FK_Medications_DosageForms_DosageFormID",
                table: "Medications");

            migrationBuilder.DropForeignKey(
                name: "FK_Medications_Suppliers_SupplierID",
                table: "Medications");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLines_Medications_MedicationID",
                table: "OrderLines");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLines_Orders_OrderID",
                table: "OrderLines");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLines_ScriptLines_ScriptLineID",
                table: "OrderLines");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerID",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Pharmacists_AspNetUsers_ApplicationUserId",
                table: "Pharmacists");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyManagers_AspNetUsers_ApplicationUserId",
                table: "PharmacyManagers");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_AspNetUsers_ApplicationUserId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_PresScriptLines_Medications_MedicationID",
                table: "PresScriptLines");

            migrationBuilder.DropForeignKey(
                name: "FK_PresScriptLines_Prescriptions_PrescriptionID",
                table: "PresScriptLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ScriptLines_Medications_MedicationID",
                table: "ScriptLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ScriptLines_Prescriptions_PrescriptionID",
                table: "ScriptLines");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "OrderLines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "OrderLines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AspNetUsers_ApplicationUserId",
                table: "Customers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Custormer_Allergies_Active_Ingredients_Active_IngredientID",
                table: "Custormer_Allergies",
                column: "Active_IngredientID",
                principalTable: "Active_Ingredients",
                principalColumn: "Active_IngredientID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Custormer_Allergies_Customers_CustomerID",
                table: "Custormer_Allergies",
                column: "CustomerID",
                principalTable: "Customers",
                principalColumn: "CustormerID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Medication_Ingredients_Active_Ingredients_Active_IngredientID",
                table: "Medication_Ingredients",
                column: "Active_IngredientID",
                principalTable: "Active_Ingredients",
                principalColumn: "Active_IngredientID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Medication_Ingredients_Medications_MedicationID",
                table: "Medication_Ingredients",
                column: "MedicationID",
                principalTable: "Medications",
                principalColumn: "MedcationID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Medications_DosageForms_DosageFormID",
                table: "Medications",
                column: "DosageFormID",
                principalTable: "DosageForms",
                principalColumn: "DosageFormID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Medications_Suppliers_SupplierID",
                table: "Medications",
                column: "SupplierID",
                principalTable: "Suppliers",
                principalColumn: "SupplierID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLines_Medications_MedicationID",
                table: "OrderLines",
                column: "MedicationID",
                principalTable: "Medications",
                principalColumn: "MedcationID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLines_Orders_OrderID",
                table: "OrderLines",
                column: "OrderID",
                principalTable: "Orders",
                principalColumn: "OrderID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLines_ScriptLines_ScriptLineID",
                table: "OrderLines",
                column: "ScriptLineID",
                principalTable: "ScriptLines",
                principalColumn: "ScriptLineID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerID",
                table: "Orders",
                column: "CustomerID",
                principalTable: "Customers",
                principalColumn: "CustormerID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pharmacists_AspNetUsers_ApplicationUserId",
                table: "Pharmacists",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyManagers_AspNetUsers_ApplicationUserId",
                table: "PharmacyManagers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_AspNetUsers_ApplicationUserId",
                table: "Prescriptions",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PresScriptLines_Medications_MedicationID",
                table: "PresScriptLines",
                column: "MedicationID",
                principalTable: "Medications",
                principalColumn: "MedcationID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PresScriptLines_Prescriptions_PrescriptionID",
                table: "PresScriptLines",
                column: "PrescriptionID",
                principalTable: "Prescriptions",
                principalColumn: "PrescriptionID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScriptLines_Medications_MedicationID",
                table: "ScriptLines",
                column: "MedicationID",
                principalTable: "Medications",
                principalColumn: "MedcationID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScriptLines_Prescriptions_PrescriptionID",
                table: "ScriptLines",
                column: "PrescriptionID",
                principalTable: "Prescriptions",
                principalColumn: "PrescriptionID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_ApplicationUserId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Custormer_Allergies_Active_Ingredients_Active_IngredientID",
                table: "Custormer_Allergies");

            migrationBuilder.DropForeignKey(
                name: "FK_Custormer_Allergies_Customers_CustomerID",
                table: "Custormer_Allergies");

            migrationBuilder.DropForeignKey(
                name: "FK_Medication_Ingredients_Active_Ingredients_Active_IngredientID",
                table: "Medication_Ingredients");

            migrationBuilder.DropForeignKey(
                name: "FK_Medication_Ingredients_Medications_MedicationID",
                table: "Medication_Ingredients");

            migrationBuilder.DropForeignKey(
                name: "FK_Medications_DosageForms_DosageFormID",
                table: "Medications");

            migrationBuilder.DropForeignKey(
                name: "FK_Medications_Suppliers_SupplierID",
                table: "Medications");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLines_Medications_MedicationID",
                table: "OrderLines");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLines_Orders_OrderID",
                table: "OrderLines");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLines_ScriptLines_ScriptLineID",
                table: "OrderLines");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerID",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Pharmacists_AspNetUsers_ApplicationUserId",
                table: "Pharmacists");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyManagers_AspNetUsers_ApplicationUserId",
                table: "PharmacyManagers");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_AspNetUsers_ApplicationUserId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_PresScriptLines_Medications_MedicationID",
                table: "PresScriptLines");

            migrationBuilder.DropForeignKey(
                name: "FK_PresScriptLines_Prescriptions_PrescriptionID",
                table: "PresScriptLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ScriptLines_Medications_MedicationID",
                table: "ScriptLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ScriptLines_Prescriptions_PrescriptionID",
                table: "ScriptLines");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "OrderLines");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OrderLines");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AspNetUsers_ApplicationUserId",
                table: "Customers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Custormer_Allergies_Active_Ingredients_Active_IngredientID",
                table: "Custormer_Allergies",
                column: "Active_IngredientID",
                principalTable: "Active_Ingredients",
                principalColumn: "Active_IngredientID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Custormer_Allergies_Customers_CustomerID",
                table: "Custormer_Allergies",
                column: "CustomerID",
                principalTable: "Customers",
                principalColumn: "CustormerID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Medication_Ingredients_Active_Ingredients_Active_IngredientID",
                table: "Medication_Ingredients",
                column: "Active_IngredientID",
                principalTable: "Active_Ingredients",
                principalColumn: "Active_IngredientID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Medication_Ingredients_Medications_MedicationID",
                table: "Medication_Ingredients",
                column: "MedicationID",
                principalTable: "Medications",
                principalColumn: "MedcationID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Medications_DosageForms_DosageFormID",
                table: "Medications",
                column: "DosageFormID",
                principalTable: "DosageForms",
                principalColumn: "DosageFormID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Medications_Suppliers_SupplierID",
                table: "Medications",
                column: "SupplierID",
                principalTable: "Suppliers",
                principalColumn: "SupplierID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLines_Medications_MedicationID",
                table: "OrderLines",
                column: "MedicationID",
                principalTable: "Medications",
                principalColumn: "MedcationID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLines_Orders_OrderID",
                table: "OrderLines",
                column: "OrderID",
                principalTable: "Orders",
                principalColumn: "OrderID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLines_ScriptLines_ScriptLineID",
                table: "OrderLines",
                column: "ScriptLineID",
                principalTable: "ScriptLines",
                principalColumn: "ScriptLineID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerID",
                table: "Orders",
                column: "CustomerID",
                principalTable: "Customers",
                principalColumn: "CustormerID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pharmacists_AspNetUsers_ApplicationUserId",
                table: "Pharmacists",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyManagers_AspNetUsers_ApplicationUserId",
                table: "PharmacyManagers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_AspNetUsers_ApplicationUserId",
                table: "Prescriptions",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PresScriptLines_Medications_MedicationID",
                table: "PresScriptLines",
                column: "MedicationID",
                principalTable: "Medications",
                principalColumn: "MedcationID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PresScriptLines_Prescriptions_PrescriptionID",
                table: "PresScriptLines",
                column: "PrescriptionID",
                principalTable: "Prescriptions",
                principalColumn: "PrescriptionID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScriptLines_Medications_MedicationID",
                table: "ScriptLines",
                column: "MedicationID",
                principalTable: "Medications",
                principalColumn: "MedcationID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScriptLines_Prescriptions_PrescriptionID",
                table: "ScriptLines",
                column: "PrescriptionID",
                principalTable: "Prescriptions",
                principalColumn: "PrescriptionID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
