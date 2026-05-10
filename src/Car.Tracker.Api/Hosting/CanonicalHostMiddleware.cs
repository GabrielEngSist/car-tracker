using Microsoft.Extensions.Options;

namespace Car.Tracker.Api.Hosting;

// 301-redirects any request whose Host header matches one of the configured "alternates" to the
// canonical host. Everything else (the canonical host itself, the .azurecontainerapps.io FQDN,
// localhost during dev, etc.) passes through untouched.
//
// Use case: keep `cartracker.app.br` (apex) and `app.cartracker.app.br` (canonical) both bound to
// the same Container App, but funnel SEO/users to the canonical hostname.
internal sealed class CanonicalHostMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string? _canonicalHost;
    private readonly HashSet<string> _alternates;

    public CanonicalHostMiddleware(RequestDelegate next, IOptionsMonitor<CanonicalHostOptions> options)
    {
        _next = next;
        var snapshot = options.CurrentValue;
        _canonicalHost = string.IsNullOrWhiteSpace(snapshot.CanonicalHost) ? null : snapshot.CanonicalHost;
        _alternates = new HashSet<string>(snapshot.RedirectAlternates, StringComparer.OrdinalIgnoreCase);
    }

    public Task InvokeAsync(HttpContext context)
    {
        // No canonical configured, or no alternates to redirect from — disabled.
        if (_canonicalHost is null || _alternates.Count == 0)
        {
            return _next(context);
        }

        var requestHost = context.Request.Host.Host;
        if (!_alternates.Contains(requestHost))
        {
            return _next(context);
        }

        // Preserve path + query, force https (the platform already terminates TLS).
        var targetUrl = $"https://{_canonicalHost}{context.Request.Path}{context.Request.QueryString}";
        context.Response.Redirect(targetUrl, permanent: true);
        return Task.CompletedTask;
    }
}

internal static class CanonicalHostMiddlewareExtensions
{
    internal static WebApplication UseCanonicalHost(this WebApplication app)
    {
        app.UseMiddleware<CanonicalHostMiddleware>();
        return app;
    }
}
