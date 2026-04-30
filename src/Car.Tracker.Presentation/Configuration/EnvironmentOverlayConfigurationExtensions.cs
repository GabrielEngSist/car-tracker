using Microsoft.Extensions.Configuration.CommandLine;

namespace Car.Tracker.Presentation.Configuration;

/// <summary>
/// O host usa por defeito ficheiros JSON e, a seguir, variáveis de ambiente (e por fim a linha de comando).
/// Esta extensão torna isso explícito: acrescenta uma fonte de variáveis de ambiente imediatamente antes
/// de qualquer fonte de linha de comando, para que o ambiente continue a sobrepor <c>appsettings</c> / user-secrets
/// sem esmagar argumentos <c>dotnet run --Key value</c>.
/// </summary>
public static class EnvironmentOverlayConfigurationExtensions
{
    /// <summary>
    /// Reordena as fontes para que exista um bloco de variáveis de ambiente imediatamente antes das fontes
    /// de linha de comando. Se não houver fonte de linha de comando, não duplica a fonte de ambiente
    /// (o host já regista uma).
    /// </summary>
    public static ConfigurationManager AddExplicitEnvironmentVariableOverlay(this ConfigurationManager configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var sources = configuration.Sources;
        var commandLineChunk = new List<IConfigurationSource>();

        for (var i = sources.Count - 1; i >= 0; i--)
        {
            if (sources[i] is CommandLineConfigurationSource)
            {
                commandLineChunk.Add(sources[i]!);
                sources.RemoveAt(i);
            }
        }

        commandLineChunk.Reverse();

        if (commandLineChunk.Count > 0)
            configuration.AddEnvironmentVariables();

        foreach (var s in commandLineChunk)
            sources.Add(s);

        return configuration;
    }
}
