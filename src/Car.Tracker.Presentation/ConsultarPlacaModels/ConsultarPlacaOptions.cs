namespace Car.Tracker.Presentation.ConsultarPlacaModels;

/// <summary>
/// Secção <see cref="SectionName"/> no config; bind com
/// <c>GetSection(ConsultarPlacaOptions.SectionName)</c>.
/// </summary>
/// <remarks>
/// Ordem (últimos ganham): appsettings → appsettings Environment → user-secrets em Development → variáveis de ambiente → CLI.
/// Exemplos env: <c>ConsultarPlaca__Email</c>, <c>ConsultarPlaca__ApiKey</c>,
/// <c>ConsultarPlaca__Url</c>; SQLite <c>ConnectionStrings__CarTracker</c>; secrets locais:
/// <c>dotnet user-secrets set "ConsultarPlaca:ApiKey" "…"</c>.
/// </remarks>
public sealed class ConsultarPlacaOptions
{
    public const string SectionName = "ConsultarPlaca";

    /// <summary>Base URL da API (ex.: https://api.consultarplaca.com.br/v2).</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Usuário para autenticação HTTP Basic.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Senha para autenticação HTTP Basic (API key).</summary>
    public string ApiKey { get; set; } = string.Empty;
}
