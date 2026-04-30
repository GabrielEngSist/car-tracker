using Car.Tracker.Presentation.Domain;
using Microsoft.EntityFrameworkCore;

namespace Car.Tracker.Presentation.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CarEntity> Cars => Set<CarEntity>();
    public DbSet<ExpenseEntry> ExpenseEntries => Set<ExpenseEntry>();
    public DbSet<MaintenancePlanItem> MaintenancePlanItems => Set<MaintenancePlanItem>();
    public DbSet<FuelingEntry> FuelingEntries => Set<FuelingEntry>();
    public DbSet<ConsultaPlaca> ConsultasPlaca => Set<ConsultaPlaca>();
    public DbSet<ConsultaPrecoFipe> ConsultasPrecoFipe => Set<ConsultaPrecoFipe>();
    public DbSet<ConsultaPrecoFipeItem> ConsultasPrecoFipeItens => Set<ConsultaPrecoFipeItem>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        StampAuditable();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditable();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampAuditable()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not AuditableEntity audit)
                continue;

            if (entry.State == EntityState.Added)
            {
                audit.CreatedAt = now;
                audit.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                audit.UpdatedAt = now;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CarEntity>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Model).HasMaxLength(512);
            b.Property(x => x.Name).HasMaxLength(200);
            b.Property(x => x.Placa).HasMaxLength(16);
            b.HasIndex(x => x.Placa);

            b.HasOne(x => x.ConsultaPlaca)
                .WithOne(x => x.Car)
                .HasForeignKey<ConsultaPlaca>(x => x.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ConsultaPrecoFipe)
                .WithOne(x => x.Car)
                .HasForeignKey<ConsultaPrecoFipe>(x => x.CarId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConsultaPlaca>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.CarId).IsUnique();
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.Mensagem).HasMaxLength(512);
            b.Property(x => x.DataSolicitacao).HasMaxLength(64);
            b.Property(x => x.RequestPlaca).HasMaxLength(16);
            b.Property(x => x.Placa).HasMaxLength(16);
            b.Property(x => x.Chassi).HasMaxLength(32);
            b.Property(x => x.AnoFabricacao).HasMaxLength(8);
            b.Property(x => x.AnoModelo).HasMaxLength(8);
            b.Property(x => x.Marca).HasMaxLength(128);
            b.Property(x => x.Modelo).HasMaxLength(512);
            b.Property(x => x.Cor).HasMaxLength(64);
            b.Property(x => x.Segmento).HasMaxLength(64);
            b.Property(x => x.Combustivel).HasMaxLength(128);
            b.Property(x => x.Procedencia).HasMaxLength(64);
            b.Property(x => x.Municipio).HasMaxLength(128);
            b.Property(x => x.UfMunicipio).HasMaxLength(4);
            b.Property(x => x.TipoVeiculo).HasMaxLength(64);
            b.Property(x => x.SubSegmento).HasMaxLength(128);
            b.Property(x => x.NumeroMotor).HasMaxLength(64);
            b.Property(x => x.NumeroCaixaCambio).HasMaxLength(64);
            b.Property(x => x.Potencia).HasMaxLength(16);
            b.Property(x => x.Cilindradas).HasMaxLength(16);
            b.Property(x => x.NumeroEixos).HasMaxLength(8);
            b.Property(x => x.CapacidadeMaximaTracao).HasMaxLength(16);
            b.Property(x => x.CapacidadePassageiro).HasMaxLength(8);
        });

        modelBuilder.Entity<ConsultaPrecoFipe>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.CarId).IsUnique();
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.Mensagem).HasMaxLength(512);
            b.Property(x => x.DataSolicitacao).HasMaxLength(64);
            b.Property(x => x.RequestPlaca).HasMaxLength(16);
            b.Property(x => x.VeiculoPlaca).HasMaxLength(16);
            b.Property(x => x.VeiculoChassi).HasMaxLength(32);
            b.Property(x => x.VeiculoAnoFabricacao).HasMaxLength(8);
            b.Property(x => x.VeiculoAnoModelo).HasMaxLength(8);
            b.Property(x => x.VeiculoMarca).HasMaxLength(128);
            b.Property(x => x.VeiculoModelo).HasMaxLength(512);
            b.Property(x => x.VeiculoCor).HasMaxLength(64);
            b.Property(x => x.VeiculoSegmento).HasMaxLength(64);
            b.Property(x => x.VeiculoCombustivel).HasMaxLength(128);
            b.Property(x => x.VeiculoProcedencia).HasMaxLength(64);
            b.Property(x => x.VeiculoMunicipio).HasMaxLength(128);
            b.Property(x => x.VeiculoUfMunicipio).HasMaxLength(4);
            b.Property(x => x.TipoVeiculo).HasMaxLength(64);
            b.Property(x => x.SubSegmento).HasMaxLength(128);
            b.Property(x => x.NumeroMotor).HasMaxLength(64);
            b.Property(x => x.NumeroCaixaCambio).HasMaxLength(64);
            b.Property(x => x.Potencia).HasMaxLength(16);
            b.Property(x => x.Cilindradas).HasMaxLength(16);
            b.Property(x => x.NumeroEixos).HasMaxLength(8);
            b.Property(x => x.CapacidadeMaximaTracao).HasMaxLength(16);
            b.Property(x => x.CapacidadePassageiro).HasMaxLength(8);
            b.Property(x => x.PesoBrutoTotal).HasMaxLength(16);

            b.HasMany(x => x.Itens)
                .WithOne(x => x.ConsultaPrecoFipe)
                .HasForeignKey(x => x.ConsultaPrecoFipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConsultaPrecoFipeItem>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.CodigoFipe).HasMaxLength(32);
            b.Property(x => x.ModeloVersao).HasMaxLength(512);
            b.Property(x => x.Preco).HasMaxLength(32);
            b.Property(x => x.MesReferencia).HasMaxLength(16);
            b.Property(x => x.HistoricoJson).HasMaxLength(20000);
        });

        modelBuilder.Entity<ExpenseEntry>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.CarId, x.PerformedAt });
            b.Property(x => x.Title).HasMaxLength(200);
            b.Property(x => x.SupplierBrand).HasMaxLength(200);
            b.Property(x => x.ProductModel).HasMaxLength(200);
            b.Property(x => x.Notes).HasMaxLength(2000);

            b.HasOne<CarEntity>()
                .WithMany()
                .HasForeignKey(x => x.CarId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MaintenancePlanItem>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.CarId, x.Active });
            b.Property(x => x.Title).HasMaxLength(200);

            b.HasOne<CarEntity>()
                .WithMany()
                .HasForeignKey(x => x.CarId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FuelingEntry>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.CarId, x.PerformedAt });
            b.Property(x => x.FuelType).HasMaxLength(64);
            b.Property(x => x.StationName).HasMaxLength(200);
            b.Property(x => x.Notes).HasMaxLength(2000);

            b.HasOne<CarEntity>()
                .WithMany()
                .HasForeignKey(x => x.CarId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
