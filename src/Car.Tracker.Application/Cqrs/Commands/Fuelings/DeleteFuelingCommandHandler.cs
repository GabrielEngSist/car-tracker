using Car.Tracker.Application.Mediator;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.Fuelings;

public sealed class DeleteFuelingCommandHandler(ITrackerPersistence db) : IRequestHandler<DeleteFuelingCommand, bool>
{
    public async Task<bool> Handle(DeleteFuelingCommand request, CancellationToken cancellationToken)
    {
        var entry = await db.GetFuelingByCarAndIdTrackedAsync(request.CarId, request.FuelingId, cancellationToken).ConfigureAwait(false);
        if (entry is null) return false;
        db.RemoveFueling(entry);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
