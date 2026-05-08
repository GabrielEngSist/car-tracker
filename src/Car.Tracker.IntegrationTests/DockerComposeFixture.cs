using System.Diagnostics;
using System.Net.Http.Json;

namespace Car.Tracker.IntegrationTests;

public sealed class DockerComposeFixture : IAsyncLifetime
{
    private static readonly string ComposeFile = "docker-compose.integration-tests.yml";
    private static readonly Uri ApiBaseUri = new("http://localhost:8085/");

    public HttpClient ApiClient { get; } = new() { BaseAddress = ApiBaseUri };
    private string? _composeFileFullPath;

    public async ValueTask InitializeAsync()
    {
        _composeFileFullPath = ResolveComposeFileFullPath();
        Run("docker", $"compose -f \"{_composeFileFullPath}\" up -d --build");
        await WaitForHealthAsync();
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(_composeFileFullPath))
                Run("docker", $"compose -f \"{_composeFileFullPath}\" down -v");
        }
        catch
        {
            // ignore cleanup failures
        }

        ApiClient.Dispose();
        return ValueTask.CompletedTask;
    }

    private static string ResolveComposeFileFullPath()
    {
        // Test runner working directory is usually bin/... so we search upwards for the repo root file.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, ComposeFile);
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        throw new FileNotFoundException($"Could not locate {ComposeFile} by walking up from {AppContext.BaseDirectory}");
    }

    private static void Run(string fileName, string args)
    {
        var psi = new ProcessStartInfo(fileName, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        using var p = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start process: {fileName}");
        p.WaitForExit();

        if (p.ExitCode != 0)
        {
            var stdout = p.StandardOutput.ReadToEnd();
            var stderr = p.StandardError.ReadToEnd();
            throw new InvalidOperationException($"Command failed ({p.ExitCode}): {fileName} {args}\n{stdout}\n{stderr}");
        }
    }

    private async Task WaitForHealthAsync()
    {
        var sw = Stopwatch.StartNew();
        Exception? last = null;

        while (sw.Elapsed < TimeSpan.FromSeconds(60))
        {
            try
            {
                using var r = await ApiClient.GetAsync("api/health");
                if (!r.IsSuccessStatusCode)
                {
                    await Task.Delay(500);
                    continue;
                }

                var body = await r.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (body is not null && body.TryGetValue("status", out var s) && s == "ok")
                    return;
            }
            catch (Exception ex)
            {
                last = ex;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException("API did not become healthy in time.", last);
    }
}

