using System.Text.Json;
using Car.Tracker.Presentation.Domain;

namespace Car.Tracker.Presentation.ConsultarPlacaModels;

/// <summary>Mapeia a resposta da API consultarPrecoFipe para <see cref="ConsultaPrecoFipe"/> + itens.</summary>
public static class ConsultarPrecoFipeMapper
{
    private static readonly JsonSerializerOptions SerializeHistorico = new();

    public static ConsultaPrecoFipe ToConsultaPrecoFipe(Guid carId, ConsultarPrecoFipeResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var row = new ConsultaPrecoFipe { CarId = carId };
        row.Status = response.Status;
        row.Mensagem = response.Mensagem;
        row.DataSolicitacao = response.DataSolicitacao;
        row.RequestPlaca = response.Request?.Placa;

        var dv = response.Dados?.InformacoesVeiculo?.DadosVeiculo;
        if (dv is not null)
        {
            row.VeiculoPlaca = dv.Placa;
            row.VeiculoChassi = dv.Chassi;
            row.VeiculoAnoFabricacao = dv.AnoFrabricacao;
            row.VeiculoAnoModelo = dv.AnoModelo;
            row.VeiculoMarca = dv.Marca;
            row.VeiculoModelo = dv.Modelo;
            row.VeiculoCor = dv.Cor;
            row.VeiculoSegmento = dv.Segmento;
            row.VeiculoCombustivel = dv.Combustivel;
            row.VeiculoProcedencia = dv.Procedencia;
            row.VeiculoMunicipio = dv.Municipio;
            row.VeiculoUfMunicipio = dv.UfMunicipio;
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
            row.PesoBrutoTotal = dc.PesoBrutoTotal;
        }

        foreach (var it in response.Dados?.InformacoesFipe ?? [])
        {
            row.Itens.Add(new ConsultaPrecoFipeItem
            {
                CodigoFipe = it.CodigoFipe,
                ModeloVersao = it.ModeloVersao,
                Preco = it.Preco,
                MesReferencia = it.MesReferencia,
                HistoricoJson = it.Historico is { Count: > 0 }
                    ? JsonSerializer.Serialize(it.Historico, SerializeHistorico)
                    : null,
            });
        }

        return row;
    }
}
