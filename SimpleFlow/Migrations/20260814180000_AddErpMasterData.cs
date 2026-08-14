using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SimpleFlow.Data;

#nullable disable

namespace SimpleFlow.Migrations;

[DbContext(typeof(SimpleFlowContext))]
[Migration("20260814180000_AddErpMasterData")]
public class AddErpMasterData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Customers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                TaxNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Customers", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Sku = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Products", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Warehouses",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Warehouses", x => x.Id));

        migrationBuilder.CreateIndex("IX_Customers_TaxNumber", "Customers", "TaxNumber", unique: true, filter: "[TaxNumber] IS NOT NULL");
        migrationBuilder.CreateIndex("IX_Products_Sku", "Products", "Sku", unique: true);
        migrationBuilder.CreateIndex("IX_Warehouses_Code", "Warehouses", "Code", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Customers");
        migrationBuilder.DropTable("Products");
        migrationBuilder.DropTable("Warehouses");
    }
}
