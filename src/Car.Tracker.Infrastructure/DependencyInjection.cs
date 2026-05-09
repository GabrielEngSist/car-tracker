using Car.Tracker.Domain.Ports.Integration;
using Car.Tracker.Domain.Ports.Persistence;
using Car.Tracker.Infrastructure.Data;
using Car.Tracker.Infrastructure.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Car.Tracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, Func<IConfiguration, string> connectionStringResolver, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionStringResolver(configuration)));
        services.AddScoped<ITrackerPersistence, EfTrackerPersistence>();
        return services;
    }

    /// <summary>HTTP clients for consultarPlaca / consultarPrecoFipe (same base URL + auth).</summary>
    public static IServiceCollection AddConsultarPlacaIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ConsultarPlacaOptions>(configuration.GetSection(ConsultarPlacaOptions.SectionName));
        services.AddTransient<ConsultarPlacaAuthHandler>();
        services.AddHttpClient<IConsultarPlacaPort, ConsultarPlacaHttpClient>((sp, client) =>
        {
            var o = sp.GetRequiredService<IOptions<ConsultarPlacaOptions>>().Value;
            var url = o.Url?.Trim();
            if (string.IsNullOrWhiteSpace(url))
                return;
            client.BaseAddress = new Uri(url.EndsWith('/') ? url : url + "/", UriKind.Absolute);
        })
        .AddHttpMessageHandler<ConsultarPlacaAuthHandler>();

        services.AddHttpClient<IConsultarPrecoFipePort, ConsultarPrecoFipeHttpClient>((sp, client) =>
        {
            var o = sp.GetRequiredService<IOptions<ConsultarPlacaOptions>>().Value;
            var url = o.Url?.Trim();
            if (string.IsNullOrWhiteSpace(url))
                return;
            client.BaseAddress = new Uri(url.EndsWith('/') ? url : url + "/", UriKind.Absolute);
        })
        .AddHttpMessageHandler<ConsultarPlacaAuthHandler>();

        return services;
    }

    public static async Task MigrateDatabaseAsync(this IServiceProvider provider, CancellationToken cancellationToken = default)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
}
