using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Queries.ExpenseEntries;

public sealed class GetCarExpenseEntriesQueryHandler(ITrackerPersistence db) : IRequestHandler<GetCarExpenseEntriesQuery, IReadOnlyList<ExpenseEntryDto>?>
{
    public async Task<HandlerResult<IReadOnlyList<ExpenseEntryDto>?>> Handle(GetCarExpenseEntriesQuery request, CancellationToken cancellationToken)
    {
        var exists = await db.CarExistsAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (!exists) return RequestOutcome.Ok<IReadOnlyList<ExpenseEntryDto>?>(null);

        var list = await db.GetExpenseEntriesForRegistryAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        var dtos = list
            .Select(x => new ExpenseEntryDto(
                x.Id, x.CarId, x.Type, x.Title, x.Price, x.SupplierBrand, x.ProductModel, x.PerformedAt, x.KmAtService, x.Notes))
            .ToList();
        return RequestOutcome.Ok<IReadOnlyList<ExpenseEntryDto>?>(dtos);
    }
}
