using Car.Tracker.Domain.Integration.Wire;

namespace Car.Tracker.Domain.Ports.Integration;

/// <summary>Outbound port for external FIPE price lookup.</summary>
public interface IConsultarPrecoFipePort
{
    Task<ConsultarPrecoFipeResponse?> ConsultarPorPlacaAsync(string placa, CancellationToken cancellationToken = default);
}
