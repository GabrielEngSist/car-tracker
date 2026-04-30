using Car.Tracker.Presentation.Domain;

namespace Car.Tracker.Presentation.Api;

/// <param name="AutoBuscarDados">Se true, exige <see cref="Placa"/> válida e chama as APIs consultarPlaca + consultarPrecoFipe (custo).</param>
public sealed record CreateCarRequest(string? Model, int? Year, int CurrentKm, string? Name, string? Placa, bool AutoBuscarDados);

public sealed record UpdateCarRequest(string? Model, int? Year, int? CurrentKm, string? Name, string? Placa);

public sealed record CarDto(Guid Id, string Model, int Year, int CurrentKm, string? Name, string? Placa, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public sealed record ConsultaPlacaDto(
    Guid Id,
    Guid CarId,
    string? Status,
    string? Mensagem,
    string? DataSolicitacao,
    string? RequestPlaca,
    string? Placa,
    string? Chassi,
    string? AnoFabricacao,
    string? AnoModelo,
    string? Marca,
    string? Modelo,
    string? Cor,
    string? Segmento,
    string? Combustivel,
    string? Procedencia,
    string? Municipio,
    string? UfMunicipio,
    string? TipoVeiculo,
    string? SubSegmento,
    string? NumeroMotor,
    string? NumeroCaixaCambio,
    string? Potencia,
    string? Cilindradas,
    string? NumeroEixos,
    string? CapacidadeMaximaTracao,
    string? CapacidadePassageiro,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ConsultaPrecoFipeItemDto(
    Guid Id,
    Guid ConsultaPrecoFipeId,
    string? CodigoFipe,
    string? ModeloVersao,
    string? Preco,
    string? MesReferencia,
    string? HistoricoJson,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ConsultaPrecoFipeDto(
    Guid Id,
    Guid CarId,
    string? Status,
    string? Mensagem,
    string? DataSolicitacao,
    string? RequestPlaca,
    string? VeiculoPlaca,
    string? VeiculoChassi,
    string? VeiculoAnoFabricacao,
    string? VeiculoAnoModelo,
    string? VeiculoMarca,
    string? VeiculoModelo,
    string? VeiculoCor,
    string? VeiculoSegmento,
    string? VeiculoCombustivel,
    string? VeiculoProcedencia,
    string? VeiculoMunicipio,
    string? VeiculoUfMunicipio,
    string? TipoVeiculo,
    string? SubSegmento,
    string? NumeroMotor,
    string? NumeroCaixaCambio,
    string? Potencia,
    string? Cilindradas,
    string? NumeroEixos,
    string? CapacidadeMaximaTracao,
    string? CapacidadePassageiro,
    string? PesoBrutoTotal,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ConsultaPrecoFipeItemDto> Itens);

public sealed record CreateExpenseEntryRequest(
    ExpenseEntryType Type,
    string Title,
    decimal Price,
    string? SupplierBrand,
    string? ProductModel,
    DateOnly PerformedAt,
    int KmAtService,
    string? Notes);

public sealed record ExpenseEntryDto(
    Guid Id,
    Guid CarId,
    ExpenseEntryType Type,
    string Title,
    decimal Price,
    string? SupplierBrand,
    string? ProductModel,
    DateOnly PerformedAt,
    int KmAtService,
    string? Notes);

public sealed record UpdateExpenseEntryRequest(
    ExpenseEntryType? Type,
    string? Title,
    decimal? Price,
    string? SupplierBrand,
    string? ProductModel,
    DateOnly? PerformedAt,
    int? KmAtService,
    string? Notes);

public sealed record CreateFuelingEntryRequest(
    DateOnly PerformedAt,
    int KmAtFueling,
    decimal Liters,
    decimal TotalPrice,
    FuelType? FuelType,
    string? StationName,
    string? Notes);

public sealed record FuelingEntryDto(
    Guid Id,
    Guid CarId,
    DateOnly PerformedAt,
    int KmAtFueling,
    decimal Liters,
    decimal TotalPrice,
    FuelType FuelType,
    string? StationName,
    string? Notes);

public sealed record UpdateFuelingEntryRequest(
    DateOnly? PerformedAt,
    int? KmAtFueling,
    decimal? Liters,
    decimal? TotalPrice,
    FuelType? FuelType,
    string? StationName,
    string? Notes);

public sealed record CreateMaintenancePlanItemRequest(
    string Title,
    int? DueKmInterval,
    int? DueTimeIntervalDays,
    bool Active);

public sealed record UpdateMaintenancePlanItemRequest(
    string? Title,
    int? DueKmInterval,
    int? DueTimeIntervalDays,
    bool? Active);

public sealed record MaintenancePlanItemDto(
    Guid Id,
    Guid CarId,
    string Title,
    int? DueKmInterval,
    int? DueTimeIntervalDays,
    bool Active);

/// <summary>Dados do carro e de tudo que está ligado a ele (consultas persistidas, lançamentos, plano).</summary>
public sealed record CarRegistryDto(
    CarDto Car,
    ConsultaPlacaDto? ConsultaPlaca,
    ConsultaPrecoFipeDto? ConsultaPrecoFipe,
    IReadOnlyList<ExpenseEntryDto> ExpenseEntries,
    IReadOnlyList<MaintenancePlanItemDto> MaintenancePlanItems);

public sealed record MaintenanceStatusDto(
    Guid PlanItemId,
    string Title,
    int? DueKmInterval,
    int? DueTimeIntervalDays,
    DateOnly? LastPerformedAt,
    int? LastKmAtService,
    DateOnly? NextDueDate,
    int? NextDueKm,
    bool OverdueByTime,
    bool OverdueByKm,
    bool Overdue);

