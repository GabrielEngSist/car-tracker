using Car.Tracker.Presentation.Domain;

namespace Car.Tracker.Presentation.Api;

internal static class CarRegistryMapper
{
    public static CarRegistryDto ToRegistry(
        CarEntity car,
        IReadOnlyList<ExpenseEntryDto> expenseEntries,
        IReadOnlyList<MaintenancePlanItemDto> maintenancePlanItems)
    {
        var carDto = new CarDto(car.Id, car.Model, car.Year, car.CurrentKm, car.Name, car.Placa, car.CreatedAt, car.UpdatedAt);
        return new CarRegistryDto(
            carDto,
            car.ConsultaPlaca is null ? null : ToConsultaPlacaDto(car.ConsultaPlaca),
            car.ConsultaPrecoFipe is null ? null : ToConsultaPrecoFipeDto(car.ConsultaPrecoFipe),
            expenseEntries,
            maintenancePlanItems);
    }

    private static ConsultaPlacaDto ToConsultaPlacaDto(ConsultaPlaca x) =>
        new(
            x.Id,
            x.CarId,
            x.Status,
            x.Mensagem,
            x.DataSolicitacao,
            x.RequestPlaca,
            x.Placa,
            x.Chassi,
            x.AnoFabricacao,
            x.AnoModelo,
            x.Marca,
            x.Modelo,
            x.Cor,
            x.Segmento,
            x.Combustivel,
            x.Procedencia,
            x.Municipio,
            x.UfMunicipio,
            x.TipoVeiculo,
            x.SubSegmento,
            x.NumeroMotor,
            x.NumeroCaixaCambio,
            x.Potencia,
            x.Cilindradas,
            x.NumeroEixos,
            x.CapacidadeMaximaTracao,
            x.CapacidadePassageiro,
            x.CreatedAt,
            x.UpdatedAt);

    private static ConsultaPrecoFipeDto ToConsultaPrecoFipeDto(ConsultaPrecoFipe x)
    {
        var itens = x.Itens
            .OrderBy(i => i.CodigoFipe)
            .ThenBy(i => i.ModeloVersao)
            .Select(i => new ConsultaPrecoFipeItemDto(
                i.Id,
                i.ConsultaPrecoFipeId,
                i.CodigoFipe,
                i.ModeloVersao,
                i.Preco,
                i.MesReferencia,
                i.HistoricoJson,
                i.CreatedAt,
                i.UpdatedAt))
            .ToList();

        return new ConsultaPrecoFipeDto(
            x.Id,
            x.CarId,
            x.Status,
            x.Mensagem,
            x.DataSolicitacao,
            x.RequestPlaca,
            x.VeiculoPlaca,
            x.VeiculoChassi,
            x.VeiculoAnoFabricacao,
            x.VeiculoAnoModelo,
            x.VeiculoMarca,
            x.VeiculoModelo,
            x.VeiculoCor,
            x.VeiculoSegmento,
            x.VeiculoCombustivel,
            x.VeiculoProcedencia,
            x.VeiculoMunicipio,
            x.VeiculoUfMunicipio,
            x.TipoVeiculo,
            x.SubSegmento,
            x.NumeroMotor,
            x.NumeroCaixaCambio,
            x.Potencia,
            x.Cilindradas,
            x.NumeroEixos,
            x.CapacidadeMaximaTracao,
            x.CapacidadePassageiro,
            x.PesoBrutoTotal,
            x.CreatedAt,
            x.UpdatedAt,
            itens);
    }
}
