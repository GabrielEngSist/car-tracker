using System.Net;
using System.Net.Http.Json;
using Npgsql;

namespace Car.Tracker.IntegrationTests;

public sealed class ApiSmokeAndDbSeedTests : IClassFixture<DockerComposeFixture>
{
    private readonly DockerComposeFixture _fx;

    public ApiSmokeAndDbSeedTests(DockerComposeFixture fx) => _fx = fx;

    [Fact]
    public async Task Health_is_ok_and_db_seeded_data_is_visible_via_api()
    {
        await SeedDatabaseAsync();

        using var r = await _fx.ApiClient.GetAsync("api/cars");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var cars = await r.Content.ReadFromJsonAsync<List<CarDto>>();
        Assert.NotNull(cars);
        Assert.Single(cars!, c => c.Model == "Seeded" && c.Year == 2020);
    }

    private static async Task SeedDatabaseAsync()
    {
        await using var conn = new NpgsqlConnection("Host=localhost;Port=5433;Database=cartracker;Username=car;Password=car;SSL Mode=Disable");
        await conn.OpenAsync();

        // Tables are created by EF migrations on API startup (quoted identifiers).
        var carId = Guid.NewGuid();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = """
INSERT INTO "Cars" ("Id","Name","Model","Year","CurrentKm","Placa","CreatedAt","UpdatedAt")
VALUES (@id, NULL, 'Seeded', 2020, 123, 'ABC1D23', NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;
""";
            cmd.Parameters.AddWithValue("id", carId);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private sealed record CarDto(
        Guid Id,
        string Model,
        int Year,
        int CurrentKm,
        string? Name,
        string? Placa,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);
}

