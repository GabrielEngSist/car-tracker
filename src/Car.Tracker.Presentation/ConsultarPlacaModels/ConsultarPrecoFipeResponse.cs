using System.Text.Json.Serialization;

namespace Car.Tracker.Presentation.ConsultarPlacaModels;

/// <summary>
/// Corpo JSON de sucesso (HTTP 200) conforme GETconsultarPrecoFipe.json.
/// </summary>
public sealed class ConsultarPrecoFipeResponse
{
    public string? Status { get; set; }
    public string? Mensagem { get; set; }

    [JsonPropertyName("data_solicitacao")]
    public string? DataSolicitacao { get; set; }

    public ConsultarPrecoFipeDadosRoot? Dados { get; set; }
    public ConsultarPrecoFipeRequestEcho? Request { get; set; }
}

public sealed class ConsultarPrecoFipeDadosRoot
{
    [JsonPropertyName("informacoes_veiculo")]
    public ConsultarPrecoFipeInformacoesVeiculo? InformacoesVeiculo { get; set; }

    [JsonPropertyName("informacoes_fipe")]
    public List<ConsultarPrecoFipeItem>? InformacoesFipe { get; set; }
}

public sealed class ConsultarPrecoFipeInformacoesVeiculo
{
    [JsonPropertyName("dados_veiculo")]
    public ConsultarPrecoFipeDadosVeiculo? DadosVeiculo { get; set; }

    [JsonPropertyName("dados_tecnicos")]
    public ConsultarPrecoFipeDadosTecnicos? DadosTecnicos { get; set; }

    [JsonPropertyName("dados_carga")]
    public ConsultarPrecoFipeDadosCarga? DadosCarga { get; set; }
}

/// <summary>
/// No exemplo da API o campo de ano de fabricação vem como <c>ano_frabricacao</c> (typo).
/// </summary>
public sealed class ConsultarPrecoFipeDadosVeiculo
{
    public string? Placa { get; set; }
    public string? Chassi { get; set; }

    [JsonPropertyName("ano_frabricacao")]
    public string? AnoFrabricacao { get; set; }

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

public sealed class ConsultarPrecoFipeDadosTecnicos
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

public sealed class ConsultarPrecoFipeDadosCarga
{
    [JsonPropertyName("numero_eixos")]
    public string? NumeroEixos { get; set; }

    [JsonPropertyName("capacidade_maxima_tracao")]
    public string? CapacidadeMaximaTracao { get; set; }

    [JsonPropertyName("capacidade_passageiro")]
    public string? CapacidadePassageiro { get; set; }

    [JsonPropertyName("peso_bruto_total")]
    public string? PesoBrutoTotal { get; set; }
}

public sealed class ConsultarPrecoFipeItem
{
    [JsonPropertyName("codigo_fipe")]
    public string? CodigoFipe { get; set; }

    [JsonPropertyName("modelo_versao")]
    public string? ModeloVersao { get; set; }

    public string? Preco { get; set; }

    [JsonPropertyName("mes_referencia")]
    public string? MesReferencia { get; set; }

    /// <summary>Chaves no formato YYYY_MM com valores string de preço.</summary>
    public Dictionary<string, string>? Historico { get; set; }
}

public sealed class ConsultarPrecoFipeRequestEcho
{
    public string? Placa { get; set; }
}
