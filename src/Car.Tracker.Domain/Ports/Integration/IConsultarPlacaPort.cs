using Car.Tracker.Domain.Integration.Wire;

namespace Car.Tracker.Domain.Ports.Integration;

/// <summary>Outbound port for external vehicle plate lookup (anti-corruption boundary).</summary>
public interface IConsultarPlacaPort
{
    Task<ConsultarPlacaResponse?> ConsultarPorPlacaAsync(string placa, CancellationToken cancellationToken = default);
}
