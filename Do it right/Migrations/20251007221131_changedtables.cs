using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IbhayiPharmacy.Migrations
{
    /// <inheritdoc />
    public partial class changedtables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MedicationID",
                table: "OrderLines",
                newName: "MedicationsMedcationID");

            migrationBuilder.AlterColumn<int>(
                name: "PharmacistID",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "MedcationID",
                table: "OrderLines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerID",
                table: "Orders",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PharmacistID",
                table: "Orders",
                column: "PharmacistID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_MedicationsMedcationID",
                table: "OrderLines",
                column: "MedicationsMedcationID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_OrderID",
                table: "OrderLines",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_ScriptLineID",
                table: "OrderLines",
                column: "ScriptLineID");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLines_Medications_MedicationsMedcationID",
                table: "OrderLines",
                column: "MedicationsMedcationID",
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
                name: "FK_Orders_Pharmacists_PharmacistID",
                table: "Orders",
                column: "PharmacistID",
                principalTable: "Pharmacists",
                principalColumn: "PharmacistID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLines_Medications_MedicationsMedcationID",
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
                name: "FK_Orders_Pharmacists_PharmacistID",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerID",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PharmacistID",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderLines_MedicationsMedcationID",
                table: "OrderLines");

            migrationBuilder.DropIndex(
                name: "IX_OrderLines_OrderID",
                table: "OrderLines");

            migrationBuilder.DropIndex(
                name: "IX_OrderLines_ScriptLineID",
                table: "OrderLines");

            migrationBuilder.DropColumn(
                name: "MedcationID",
                table: "OrderLines");

            migrationBuilder.RenameColumn(
                name: "MedicationsMedcationID",
                table: "OrderLines",
                newName: "MedicationID");

            migrationBuilder.AlterColumn<int>(
                name: "PharmacistID",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
