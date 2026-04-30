using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car.Tracker.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFuelingFullTankFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFullTank",
                table: "FuelingEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFullTank",
                table: "FuelingEntries");
        }
    }
}
