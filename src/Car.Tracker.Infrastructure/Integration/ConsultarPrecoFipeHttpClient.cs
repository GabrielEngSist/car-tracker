using System.Net.Http.Json;
using System.Text.Json;
using Car.Tracker.Domain.Integration.Wire;
using Car.Tracker.Domain.Ports.Integration;

namespace Car.Tracker.Infrastructure.Integration;

public sealed class ConsultarPrecoFipeHttpClient(HttpClient http) : IConsultarPrecoFipePort
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<ConsultarPrecoFipeResponse?> ConsultarPorPlacaAsync(string placa, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(placa);

        var relative = $"consultarPrecoFipe?placa={Uri.EscapeDataString(placa.Trim())}";
        using var response = await http.GetAsync(relative, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ConsultarPrecoFipeResponse>(JsonOptions, cancellationToken).ConfigureAwait(false);
    }
}
