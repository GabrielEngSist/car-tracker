using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car.Tracker.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFuelingEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FuelingEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CarId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    KmAtFueling = table.Column<int>(type: "integer", nullable: false),
                    Liters = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    FuelType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    StationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelingEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuelingEntries_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FuelingEntries_CarId_PerformedAt",
                table: "FuelingEntries",
                columns: new[] { "CarId", "PerformedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FuelingEntries");
        }
    }
}
