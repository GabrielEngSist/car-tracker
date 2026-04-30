using System.Text.Json;

namespace Car.Tracker.Presentation.ConsultarPlacaModels;

public sealed class ConsultarPrecoFipeHttpClient(HttpClient http) : IConsultarPrecoFipeClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <inheritdoc />
    public async Task<ConsultarPrecoFipeResponse?> ConsultarPorPlacaAsync(string placa, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(placa);

        var relative = $"consultarPrecoFipe?placa={Uri.EscapeDataString(placa.Trim())}";
        using var response = await http.GetAsync(relative, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ConsultarPrecoFipeResponse>(JsonOptions, cancellationToken).ConfigureAwait(false);
    }
}
