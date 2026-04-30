using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Car.Tracker.Presentation.Data;

/// <summary>
/// Bases criadas com <c>EnsureCreated()</c> não evoluem com o modelo. Esta classe alinha SQLite legado
/// (colunas e tabelas novas) e marca a migração inicial como aplicada para que <c>Migrate()</c> funcione.
/// </summary>
public static class SqliteLegacySchemaSync
{
    private const string InitialMigrationId = "20260430005606_InitialCreate";
    private const string EfProductVersion = "10.0.7";

    public static async Task ApplyAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        var creator = db.Database.GetService<IRelationalDatabaseCreator>()
                      ?? throw new InvalidOperationException("IRelationalDatabaseCreator not available.");

        if (!await creator.HasTablesAsync(cancellationToken))
        {
            await db.Database.MigrateAsync(cancellationToken);
            return;
        }

        await db.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            var conn = db.Database.GetDbConnection();
            await EnsureColumnAsync(conn, "Cars", "Placa", """ALTER TABLE "Cars" ADD COLUMN "Placa" TEXT NULL;""", cancellationToken);
            await EnsureColumnAsync(
                conn,
                "Cars",
                "CreatedAt",
                """ALTER TABLE "Cars" ADD COLUMN "CreatedAt" TEXT NOT NULL DEFAULT '0001-01-01T00:00:00+00:00';""",
                cancellationToken);
            await EnsureColumnAsync(
                conn,
                "Cars",
                "UpdatedAt",
                """ALTER TABLE "Cars" ADD COLUMN "UpdatedAt" TEXT NOT NULL DEFAULT '0001-01-01T00:00:00+00:00';""",
                cancellationToken);

            await EnsureColumnAsync(
                conn,
                "ExpenseEntries",
                "CreatedAt",
                """ALTER TABLE "ExpenseEntries" ADD COLUMN "CreatedAt" TEXT NOT NULL DEFAULT '0001-01-01T00:00:00+00:00';""",
                cancellationToken);
            await EnsureColumnAsync(
                conn,
                "ExpenseEntries",
                "UpdatedAt",
                """ALTER TABLE "ExpenseEntries" ADD COLUMN "UpdatedAt" TEXT NOT NULL DEFAULT '0001-01-01T00:00:00+00:00';""",
                cancellationToken);

            await EnsureColumnAsync(
                conn,
                "MaintenancePlanItems",
                "CreatedAt",
                """ALTER TABLE "MaintenancePlanItems" ADD COLUMN "CreatedAt" TEXT NOT NULL DEFAULT '0001-01-01T00:00:00+00:00';""",
                cancellationToken);
            await EnsureColumnAsync(
                conn,
                "MaintenancePlanItems",
                "UpdatedAt",
                """ALTER TABLE "MaintenancePlanItems" ADD COLUMN "UpdatedAt" TEXT NOT NULL DEFAULT '0001-01-01T00:00:00+00:00';""",
                cancellationToken);

            await EnsureConsultaTablesAsync(conn, cancellationToken);
            await EnsureIndexesAsync(conn, cancellationToken);
            await EnsureMigrationHistoryAsync(conn, cancellationToken);
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }

        await db.Database.MigrateAsync(cancellationToken);
    }

    private static async Task EnsureColumnAsync(
        System.Data.Common.DbConnection conn,
        string table,
        string column,
        string alterSql,
        CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync(conn, table, cancellationToken))
            return;

        if (await ColumnExistsAsync(conn, table, column, cancellationToken))
            return;

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = alterSql;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> TableExistsAsync(System.Data.Common.DbConnection conn, string table, CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(1) FROM sqlite_master
            WHERE type = 'table' AND name = $name;
            """;
        var p = cmd.CreateParameter();
        p.ParameterName = "name";
        p.Value = table;
        cmd.Parameters.Add(p);
        var count = Convert.ToInt64(await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        return count > 0;
    }

    private static async Task<bool> ColumnExistsAsync(DbConnection conn, string table, string column, CancellationToken cancellationToken)
    {
        ThrowIfUnsafeIdent(table);
        ThrowIfUnsafeIdent(column);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info(\"{table}\");";
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var colName = reader.GetString(1);
            if (string.Equals(colName, column, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static void ThrowIfUnsafeIdent(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        foreach (var c in name)
        {
            if (!char.IsAsciiLetterOrDigit(c) && c != '_')
                throw new ArgumentException($"Invalid SQL identifier: {name}.");
        }
    }

    private static async Task EnsureConsultaTablesAsync(DbConnection conn, CancellationToken cancellationToken)
    {
        await RunSqlAsync(
            conn,
            """
            CREATE TABLE IF NOT EXISTS "ConsultasPlaca" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ConsultasPlaca" PRIMARY KEY,
                "CarId" TEXT NOT NULL,
                "Status" TEXT NULL,
                "Mensagem" TEXT NULL,
                "DataSolicitacao" TEXT NULL,
                "RequestPlaca" TEXT NULL,
                "Placa" TEXT NULL,
                "Chassi" TEXT NULL,
                "AnoFabricacao" TEXT NULL,
                "AnoModelo" TEXT NULL,
                "Marca" TEXT NULL,
                "Modelo" TEXT NULL,
                "Cor" TEXT NULL,
                "Segmento" TEXT NULL,
                "Combustivel" TEXT NULL,
                "Procedencia" TEXT NULL,
                "Municipio" TEXT NULL,
                "UfMunicipio" TEXT NULL,
                "TipoVeiculo" TEXT NULL,
                "SubSegmento" TEXT NULL,
                "NumeroMotor" TEXT NULL,
                "NumeroCaixaCambio" TEXT NULL,
                "Potencia" TEXT NULL,
                "Cilindradas" TEXT NULL,
                "NumeroEixos" TEXT NULL,
                "CapacidadeMaximaTracao" TEXT NULL,
                "CapacidadePassageiro" TEXT NULL,
                "CreatedAt" TEXT NOT NULL,
                "UpdatedAt" TEXT NOT NULL,
                CONSTRAINT "FK_ConsultasPlaca_Cars_CarId" FOREIGN KEY ("CarId") REFERENCES "Cars" ("Id") ON DELETE CASCADE
            );
            """,
            cancellationToken);

        await RunSqlAsync(
            conn,
            """
            CREATE TABLE IF NOT EXISTS "ConsultasPrecoFipe" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ConsultasPrecoFipe" PRIMARY KEY,
                "CarId" TEXT NOT NULL,
                "Status" TEXT NULL,
                "Mensagem" TEXT NULL,
                "DataSolicitacao" TEXT NULL,
                "RequestPlaca" TEXT NULL,
                "VeiculoPlaca" TEXT NULL,
                "VeiculoChassi" TEXT NULL,
                "VeiculoAnoFabricacao" TEXT NULL,
                "VeiculoAnoModelo" TEXT NULL,
                "VeiculoMarca" TEXT NULL,
                "VeiculoModelo" TEXT NULL,
                "VeiculoCor" TEXT NULL,
                "VeiculoSegmento" TEXT NULL,
                "VeiculoCombustivel" TEXT NULL,
                "VeiculoProcedencia" TEXT NULL,
                "VeiculoMunicipio" TEXT NULL,
                "VeiculoUfMunicipio" TEXT NULL,
                "TipoVeiculo" TEXT NULL,
                "SubSegmento" TEXT NULL,
                "NumeroMotor" TEXT NULL,
                "NumeroCaixaCambio" TEXT NULL,
                "Potencia" TEXT NULL,
                "Cilindradas" TEXT NULL,
                "NumeroEixos" TEXT NULL,
                "CapacidadeMaximaTracao" TEXT NULL,
                "CapacidadePassageiro" TEXT NULL,
                "PesoBrutoTotal" TEXT NULL,
                "CreatedAt" TEXT NOT NULL,
                "UpdatedAt" TEXT NOT NULL,
                CONSTRAINT "FK_ConsultasPrecoFipe_Cars_CarId" FOREIGN KEY ("CarId") REFERENCES "Cars" ("Id") ON DELETE CASCADE
            );
            """,
            cancellationToken);

        await RunSqlAsync(
            conn,
            """
            CREATE TABLE IF NOT EXISTS "ConsultasPrecoFipeItens" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ConsultasPrecoFipeItens" PRIMARY KEY,
                "ConsultaPrecoFipeId" TEXT NOT NULL,
                "CodigoFipe" TEXT NULL,
                "ModeloVersao" TEXT NULL,
                "Preco" TEXT NULL,
                "MesReferencia" TEXT NULL,
                "HistoricoJson" TEXT NULL,
                "CreatedAt" TEXT NOT NULL,
                "UpdatedAt" TEXT NOT NULL,
                CONSTRAINT "FK_ConsultasPrecoFipeItens_ConsultasPrecoFipe_ConsultaPrecoFipeId" FOREIGN KEY ("ConsultaPrecoFipeId") REFERENCES "ConsultasPrecoFipe" ("Id") ON DELETE CASCADE
            );
            """,
            cancellationToken);
    }

    private static async Task EnsureIndexesAsync(DbConnection conn, CancellationToken cancellationToken)
    {
        await RunSqlAsync(conn, """CREATE INDEX IF NOT EXISTS "IX_Cars_Placa" ON "Cars" ("Placa");""", cancellationToken);
        await RunSqlAsync(conn, """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConsultasPlaca_CarId" ON "ConsultasPlaca" ("CarId");""", cancellationToken);
        await RunSqlAsync(conn, """CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConsultasPrecoFipe_CarId" ON "ConsultasPrecoFipe" ("CarId");""", cancellationToken);
        await RunSqlAsync(
            conn,
            """CREATE INDEX IF NOT EXISTS "IX_ConsultasPrecoFipeItens_ConsultaPrecoFipeId" ON "ConsultasPrecoFipeItens" ("ConsultaPrecoFipeId");""",
            cancellationToken);
        await RunSqlAsync(
            conn,
            """CREATE INDEX IF NOT EXISTS "IX_ExpenseEntries_CarId_PerformedAt" ON "ExpenseEntries" ("CarId", "PerformedAt");""",
            cancellationToken);
        await RunSqlAsync(
            conn,
            """CREATE INDEX IF NOT EXISTS "IX_MaintenancePlanItems_CarId_Active" ON "MaintenancePlanItems" ("CarId", "Active");""",
            cancellationToken);
    }

    private static async Task EnsureMigrationHistoryAsync(DbConnection conn, CancellationToken cancellationToken)
    {
        await RunSqlAsync(
            conn,
            """
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                "ProductVersion" TEXT NOT NULL
            );
            """,
            cancellationToken);

        await using (var check = conn.CreateCommand())
        {
            check.CommandText = """SELECT COUNT(1) FROM "__EFMigrationsHistory" WHERE "MigrationId" = $mid;""";
            var pm = check.CreateParameter();
            pm.ParameterName = "mid";
            pm.Value = InitialMigrationId;
            check.Parameters.Add(pm);
            var already = Convert.ToInt64(await check.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
            if (already > 0)
                return;
        }

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ($mid, $pv);
            """;
        var p1 = cmd.CreateParameter();
        p1.ParameterName = "mid";
        p1.Value = InitialMigrationId;
        cmd.Parameters.Add(p1);
        var p2 = cmd.CreateParameter();
        p2.ParameterName = "pv";
        p2.Value = EfProductVersion;
        cmd.Parameters.Add(p2);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task RunSqlAsync(DbConnection conn, string sql, CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
