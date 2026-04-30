using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car.Tracker.Presentation.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeFuelingFuelTypeEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
UPDATE "FuelingEntries"
SET "FuelType" = 'Gasolina'
WHERE "FuelType" IS NULL OR TRIM("FuelType") = '' OR UPPER("FuelType") NOT IN ('GASOLINA','ALCOOL','DIESEL','KV');
""");

            migrationBuilder.AlterColumn<string>(
                name: "FuelType",
                table: "FuelingEntries",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Gasolina",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FuelType",
                table: "FuelingEntries",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);
        }
    }
}
