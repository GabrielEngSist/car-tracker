using Car.Tracker.Application.Mediator;

namespace Car.Tracker.Application.Cqrs.Commands.Fuelings;

public sealed class CreateFuelingCommandValidator : IValidator<CreateFuelingCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(
        CreateFuelingCommand request,
        CancellationToken cancellationToken = default)
    {
        var body = request.Body;
        var faults = new List<FaultDetail>();

        if (body.KmAtFueling < 0)
            faults.Add(new FaultDetail("INVALID_KM", nameof(body.KmAtFueling), "KmAtFueling is invalid."));
        if (body.Liters <= 0)
            faults.Add(new FaultDetail("INVALID_LITERS", nameof(body.Liters), "Liters must be > 0."));
        if (body.TotalPrice < 0)
            faults.Add(new FaultDetail("INVALID_TOTAL_PRICE", nameof(body.TotalPrice), "TotalPrice is invalid."));

        return new ValueTask<ValidationResult>(
            faults.Count == 0 ? ValidationResult.Ok : ValidationResult.FromFaults(faults));
    }
}
