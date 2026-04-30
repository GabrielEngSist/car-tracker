namespace Car.Tracker.Presentation.ConsultarPlacaModels;

/// <summary>
/// Cliente HTTP para GET <c>consultarPlaca</c> com parâmetro obrigatório <c>placa</c> (formatos AAA0000 ou AAA09A00).
/// Chamadas geram custo no provedor — não invoque em startup, testes de integração sem mock, ou loops.
/// </summary>
public interface IConsultarPlacaClient
{
    /// <summary>GET consultarPlaca?placa=...</summary>
    Task<ConsultarPlacaResponse?> ConsultarPorPlacaAsync(string placa, CancellationToken cancellationToken = default);
}
