using Car.Tracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Car.Tracker.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<CarEntity> Cars { get; }
    DbSet<ExpenseEntry> ExpenseEntries { get; }
    DbSet<MaintenancePlanItem> MaintenancePlanItems { get; }
    DbSet<FuelingEntry> FuelingEntries { get; }
    DbSet<ConsultaPlaca> ConsultasPlaca { get; }
    DbSet<ConsultaPrecoFipe> ConsultasPrecoFipe { get; }
    DbSet<ConsultaPrecoFipeItem> ConsultasPrecoFipeItens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

