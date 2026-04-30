using System.Text.Json;

namespace Car.Tracker.Api.ConsultarPlacaModels;

public sealed class ConsultarPlacaHttpClient(HttpClient http) : IConsultarPlacaClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <inheritdoc />
    public async Task<ConsultarPlacaResponse?> ConsultarPorPlacaAsync(string placa, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(placa);

        var relative = $"consultarPlaca?placa={Uri.EscapeDataString(placa.Trim())}";
        using var response = await http.GetAsync(relative, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ConsultarPlacaResponse>(JsonOptions, cancellationToken).ConfigureAwait(false);
    }
}
