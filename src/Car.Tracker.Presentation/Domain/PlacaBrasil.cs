using System.Text.RegularExpressions;

namespace Car.Tracker.Presentation.Domain;

/// <summary>Validação e normalização de placas no padrão brasileiro (Mercosul ou antigo).</summary>
public static partial class PlacaBrasil
{
    /// <summary>Remove hífen, trim e caixa alta.</summary>
    public static string Normalizar(string placa) =>
        placa.Trim().Replace("-", "", StringComparison.Ordinal).ToUpperInvariant();

    /// <summary>Antigo (AAA0000), Mercosul 7 (ABC1D23) ou 8 caracteres (AAA09A00), conforme documentação comum.</summary>
    public static bool EhValida(string normalizada)
    {
        if (string.IsNullOrEmpty(normalizada) || normalizada.Length is < 7 or > 8)
            return false;

        return PlacaAntiga().IsMatch(normalizada)
               || PlacaMercosul7().IsMatch(normalizada);
    }

    [GeneratedRegex("^[A-Z]{3}[0-9]{4}$", RegexOptions.CultureInvariant)]
    private static partial Regex PlacaAntiga();

    [GeneratedRegex("^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$", RegexOptions.CultureInvariant)]
    private static partial Regex PlacaMercosul7();
}
