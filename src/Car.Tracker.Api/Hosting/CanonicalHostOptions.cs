namespace Car.Tracker.Api.Hosting;

// Bound from configuration section "Hosting". Container Apps injects values via env vars:
//   Hosting__CanonicalHost           -> the host visitors SHOULD be on
//   Hosting__RedirectAlternatesCsv   -> comma-separated list of hosts to 301 to canonical
//
// Comma-separated string (instead of an array) keeps Container Apps env config flat: a single env
// var per setting maps cleanly to IConfiguration without `Hosting__RedirectAlternates__0` indexing.
internal sealed class CanonicalHostOptions
{
    internal const string SectionName = "Hosting";

    public string? CanonicalHost { get; init; }

    public string? RedirectAlternatesCsv { get; init; }

    public IReadOnlyList<string> RedirectAlternates =>
        string.IsNullOrWhiteSpace(RedirectAlternatesCsv)
            ? Array.Empty<string>()
            : RedirectAlternatesCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToArray();
}
