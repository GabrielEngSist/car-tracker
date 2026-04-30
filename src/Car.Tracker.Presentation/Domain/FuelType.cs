namespace Car.Tracker.Presentation.Domain;

/// <summary>Tipos de combustível aceitos pelo sistema.</summary>
public enum FuelType
{
    Gasolina = 1,
    Alcool = 2,
    Diesel = 3,
    /// <summary>Veículos elétricos (KV como unidade equivalente solicitada).</summary>
    KV = 4,
}
