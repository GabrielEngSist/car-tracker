using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car.Tracker.Presentation.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Model = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentKm = table.Column<int>(type: "INTEGER", nullable: false),
                    Placa = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsultasPlaca",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CarId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Mensagem = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    DataSolicitacao = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    RequestPlaca = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Placa = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Chassi = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    AnoFabricacao = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    AnoModelo = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    Marca = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Modelo = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    Cor = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Segmento = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Combustivel = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Procedencia = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Municipio = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    UfMunicipio = table.Column<string>(type: "TEXT", maxLength: 4, nullable: true),
                    TipoVeiculo = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    SubSegmento = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    NumeroMotor = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    NumeroCaixaCambio = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Potencia = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Cilindradas = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    NumeroEixos = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    CapacidadeMaximaTracao = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    CapacidadePassageiro = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultasPlaca", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultasPlaca_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsultasPrecoFipe",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CarId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Mensagem = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    DataSolicitacao = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    RequestPlaca = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    VeiculoPlaca = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    VeiculoChassi = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    VeiculoAnoFabricacao = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    VeiculoAnoModelo = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    VeiculoMarca = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    VeiculoModelo = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    VeiculoCor = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    VeiculoSegmento = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    VeiculoCombustivel = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    VeiculoProcedencia = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    VeiculoMunicipio = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    VeiculoUfMunicipio = table.Column<string>(type: "TEXT", maxLength: 4, nullable: true),
                    TipoVeiculo = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    SubSegmento = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    NumeroMotor = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    NumeroCaixaCambio = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Potencia = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Cilindradas = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    NumeroEixos = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    CapacidadeMaximaTracao = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    CapacidadePassageiro = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    PesoBrutoTotal = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultasPrecoFipe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultasPrecoFipe_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CarId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    SupplierBrand = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ProductModel = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PerformedAt = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    KmAtService = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseEntries_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenancePlanItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CarId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DueKmInterval = table.Column<int>(type: "INTEGER", nullable: true),
                    DueTimeIntervalDays = table.Column<int>(type: "INTEGER", nullable: true),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenancePlanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenancePlanItems_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsultasPrecoFipeItens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConsultaPrecoFipeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CodigoFipe = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    ModeloVersao = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    Preco = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    MesReferencia = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    HistoricoJson = table.Column<string>(type: "TEXT", maxLength: 20000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultasPrecoFipeItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultasPrecoFipeItens_ConsultasPrecoFipe_ConsultaPrecoFipeId",
                        column: x => x.ConsultaPrecoFipeId,
                        principalTable: "ConsultasPrecoFipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cars_Placa",
                table: "Cars",
                column: "Placa");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultasPlaca_CarId",
                table: "ConsultasPlaca",
                column: "CarId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultasPrecoFipe_CarId",
                table: "ConsultasPrecoFipe",
                column: "CarId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultasPrecoFipeItens_ConsultaPrecoFipeId",
                table: "ConsultasPrecoFipeItens",
                column: "ConsultaPrecoFipeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseEntries_CarId_PerformedAt",
                table: "ExpenseEntries",
                columns: new[] { "CarId", "PerformedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenancePlanItems_CarId_Active",
                table: "MaintenancePlanItems",
                columns: new[] { "CarId", "Active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultasPlaca");

            migrationBuilder.DropTable(
                name: "ConsultasPrecoFipeItens");

            migrationBuilder.DropTable(
                name: "ExpenseEntries");

            migrationBuilder.DropTable(
                name: "MaintenancePlanItems");

            migrationBuilder.DropTable(
                name: "ConsultasPrecoFipe");

            migrationBuilder.DropTable(
                name: "Cars");
        }
    }
}
