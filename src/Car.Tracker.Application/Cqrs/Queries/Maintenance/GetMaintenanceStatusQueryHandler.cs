using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Queries.Maintenance;

public sealed class GetMaintenanceStatusQueryHandler(ITrackerPersistence db) : IRequestHandler<GetMaintenanceStatusQuery, IReadOnlyList<MaintenanceStatusDto>?>
{
    public async Task<IReadOnlyList<MaintenanceStatusDto>?> Handle(GetMaintenanceStatusQuery request, CancellationToken cancellationToken)
    {
        var car = await db.GetCarByIdReadOnlyAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (car is null) return null;

        var plans = await db.GetActiveMaintenancePlansOrderedAsync(request.CarId, cancellationToken).ConfigureAwait(false);

        var serviceEntries = await db.GetServiceExpenseEntriesForMaintenanceStatusAsync(request.CarId, cancellationToken).ConfigureAwait(false);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = plans.Select(plan =>
        {
            var last = serviceEntries.FirstOrDefault(e =>
                string.Equals(e.Title, plan.Title, StringComparison.OrdinalIgnoreCase));

            var baselineDate = last?.PerformedAt;
            var baselineKm = last?.KmAtService;

            DateOnly? nextDueDate = null;
            int? nextDueKm = null;

            if (plan.DueTimeIntervalDays is not null)
            {
                var start = baselineDate ?? DateOnly.FromDateTime(car.CreatedAt.UtcDateTime);
                nextDueDate = start.AddDays(plan.DueTimeIntervalDays.Value);
            }

            if (plan.DueKmInterval is not null)
            {
                var start = baselineKm ?? car.CurrentKm;
                nextDueKm = start + plan.DueKmInterval.Value;
            }

            var overdueByTime = nextDueDate is not null && nextDueDate.Value <= today;
            var overdueByKm = nextDueKm is not null && nextDueKm.Value <= car.CurrentKm;

            return new MaintenanceStatusDto(
                plan.Id,
                plan.Title,
                plan.DueKmInterval,
                plan.DueTimeIntervalDays,
                baselineDate,
                baselineKm,
                nextDueDate,
                nextDueKm,
                overdueByTime,
                overdueByKm,
                overdueByTime || overdueByKm);
        }).ToList();

        return result
            .OrderByDescending(x => x.Overdue)
            .ThenBy(x =>
            {
                var km = x.NextDueKm;
                var date = x.NextDueDate;
                var kmValue = km ?? int.MaxValue;
                var dateValue = date ?? DateOnly.MaxValue;
                return (dateValue.ToDateTime(TimeOnly.MinValue), kmValue);
            })
            .ToList();
    }
}
