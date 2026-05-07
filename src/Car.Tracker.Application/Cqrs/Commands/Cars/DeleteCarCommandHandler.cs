using Car.Tracker.Application.Mediator;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.Cars;

public sealed class DeleteCarCommandHandler(ITrackerPersistence db) : IRequestHandler<DeleteCarCommand, bool>
{
    public async Task<HandlerResult<bool>> Handle(DeleteCarCommand request, CancellationToken cancellationToken)
    {
        var car = await db.GetCarByIdTrackedAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (car is null) return RequestOutcome.Ok(false);
        db.RemoveCar(car);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return RequestOutcome.Ok(true);
    }
}
