namespace Car.Tracker.Infrastructure.Integration;

public sealed class ConsultarPlacaOptions
{
    public const string SectionName = "ConsultarPlaca";

    public string? Url { get; set; }
    public string? Email { get; set; }
    public string? ApiKey { get; set; }
}
