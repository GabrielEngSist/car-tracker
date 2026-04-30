using System.Text.Json.Serialization;

namespace Car.Tracker.Api.ConsultarPlacaModels;

/// <summary>
/// Corpo JSON de sucesso (HTTP 200) conforme GETconsultarPlaca.json.
/// </summary>
public sealed class ConsultarPlacaResponse
{
    public string? Status { get; set; }
    public string? Mensagem { get; set; }

    [JsonPropertyName("data_solicitacao")]
    public string? DataSolicitacao { get; set; }

    public ConsultarPlacaDadosRoot? Dados { get; set; }
    public ConsultarPlacaRequestEcho? Request { get; set; }
}

public sealed class ConsultarPlacaDadosRoot
{
    [JsonPropertyName("informacoes_veiculo")]
    public ConsultarPlacaInformacoesVeiculo? InformacoesVeiculo { get; set; }
}

public sealed class ConsultarPlacaInformacoesVeiculo
{
    [JsonPropertyName("dados_veiculo")]
    public ConsultarPlacaDadosVeiculo? DadosVeiculo { get; set; }

    [JsonPropertyName("dados_tecnicos")]
    public ConsultarPlacaDadosTecnicos? DadosTecnicos { get; set; }

    [JsonPropertyName("dados_carga")]
    public ConsultarPlacaDadosCarga? DadosCarga { get; set; }
}

public sealed class ConsultarPlacaDadosVeiculo
{
    public string? Placa { get; set; }
    public string? Chassi { get; set; }

    [JsonPropertyName("ano_fabricacao")]
    public string? AnoFabricacao { get; set; }

    [JsonPropertyName("ano_modelo")]
    public string? AnoModelo { get; set; }

    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Cor { get; set; }
    public string? Segmento { get; set; }
    public string? Combustivel { get; set; }
    public string? Procedencia { get; set; }
    public string? Municipio { get; set; }

    [JsonPropertyName("uf_municipio")]
    public string? UfMunicipio { get; set; }
}

public sealed class ConsultarPlacaDadosTecnicos
{
    [JsonPropertyName("tipo_veiculo")]
    public string? TipoVeiculo { get; set; }

    [JsonPropertyName("sub_segmento")]
    public string? SubSegmento { get; set; }

    [JsonPropertyName("numero_motor")]
    public string? NumeroMotor { get; set; }

    [JsonPropertyName("numero_caixa_cambio")]
    public string? NumeroCaixaCambio { get; set; }

    public string? Potencia { get; set; }
    public string? Cilindradas { get; set; }
}

public sealed class ConsultarPlacaDadosCarga
{
    [JsonPropertyName("numero_eixos")]
    public string? NumeroEixos { get; set; }

    [JsonPropertyName("capacidade_maxima_tracao")]
    public string? CapacidadeMaximaTracao { get; set; }

    [JsonPropertyName("capacidade_passageiro")]
    public string? CapacidadePassageiro { get; set; }
}

public sealed class ConsultarPlacaRequestEcho
{
    public string? Placa { get; set; }
}
