namespace Car.Tracker.Presentation.ConsultarPlacaModels;

/// <summary>
/// Cliente HTTP para GET <c>consultarPrecoFipe</c> com parâmetro obrigatório <c>placa</c>.
/// Chamadas geram custo — não invoque em startup ou em testes contra produção sem controle.
/// </summary>
public interface IConsultarPrecoFipeClient
{
    Task<ConsultarPrecoFipeResponse?> ConsultarPorPlacaAsync(string placa, CancellationToken cancellationToken = default);
}
