using Car.Tracker.Application.Mediator;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.ExpenseEntries;

public sealed class DeleteExpenseEntryCommandHandler(ITrackerPersistence db) : IRequestHandler<DeleteExpenseEntryCommand, bool>
{
    public async Task<bool> Handle(DeleteExpenseEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await db.GetExpenseEntryByCarAndIdTrackedAsync(request.CarId, request.EntryId, cancellationToken).ConfigureAwait(false);
        if (entry is null) return false;
        db.RemoveExpenseEntry(entry);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
