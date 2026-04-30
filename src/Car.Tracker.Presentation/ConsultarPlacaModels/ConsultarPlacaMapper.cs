using Car.Tracker.Presentation.Domain;

namespace Car.Tracker.Presentation.ConsultarPlacaModels;

/// <summary>Mapeia a resposta da API consultarPlaca para a entidade <see cref="ConsultaPlaca"/>.</summary>
public static class ConsultarPlacaMapper
{
    public static ConsultaPlaca ToConsultaPlaca(Guid carId, ConsultarPlacaResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var row = new ConsultaPlaca { CarId = carId };
        row.Status = response.Status;
        row.Mensagem = response.Mensagem;
        row.DataSolicitacao = response.DataSolicitacao;
        row.RequestPlaca = response.Request?.Placa;

        var dv = response.Dados?.InformacoesVeiculo?.DadosVeiculo;
        if (dv is not null)
        {
            row.Placa = dv.Placa;
            row.Chassi = dv.Chassi;
            row.AnoFabricacao = dv.AnoFabricacao;
            row.AnoModelo = dv.AnoModelo;
            row.Marca = dv.Marca;
            row.Modelo = dv.Modelo;
            row.Cor = dv.Cor;
            row.Segmento = dv.Segmento;
            row.Combustivel = dv.Combustivel;
            row.Procedencia = dv.Procedencia;
            row.Municipio = dv.Municipio;
            row.UfMunicipio = dv.UfMunicipio;
        }

        var dt = response.Dados?.InformacoesVeiculo?.DadosTecnicos;
        if (dt is not null)
        {
            row.TipoVeiculo = dt.TipoVeiculo;
            row.SubSegmento = dt.SubSegmento;
            row.NumeroMotor = dt.NumeroMotor;
            row.NumeroCaixaCambio = dt.NumeroCaixaCambio;
            row.Potencia = dt.Potencia;
            row.Cilindradas = dt.Cilindradas;
        }

        var dc = response.Dados?.InformacoesVeiculo?.DadosCarga;
        if (dc is not null)
        {
            row.NumeroEixos = dc.NumeroEixos;
            row.CapacidadeMaximaTracao = dc.CapacidadeMaximaTracao;
            row.CapacidadePassageiro = dc.CapacidadePassageiro;
        }

        return row;
    }

    /// <summary>Preenche <see cref="CarEntity"/> com dados principais da resposta consultarPlaca (cadastro automático).</summary>
    public static void PreencherCarro(CarEntity car, ConsultarPlacaResponse response, int currentKm, string? name, string placaNormalizada)
    {
        ArgumentNullException.ThrowIfNull(car);
        ArgumentNullException.ThrowIfNull(response);

        car.Placa = placaNormalizada;
        car.CurrentKm = currentKm;
        car.Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();

        var dv = response.Dados?.InformacoesVeiculo?.DadosVeiculo;
        car.Model = string.IsNullOrWhiteSpace(dv?.Modelo) ? "Sem modelo" : dv.Modelo.Trim();

        if (int.TryParse(dv?.AnoModelo, out var anoModelo))
            car.Year = anoModelo;
        else if (int.TryParse(dv?.AnoFabricacao, out var anoFab))
            car.Year = anoFab;
        else
            car.Year = DateTime.UtcNow.Year;
    }
}
