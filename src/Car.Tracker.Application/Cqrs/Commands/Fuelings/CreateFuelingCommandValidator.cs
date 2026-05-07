using Car.Tracker.Application.Mediator;

namespace Car.Tracker.Application.Cqrs.Commands.Fuelings;

public sealed class CreateFuelingCommandValidator : IValidator<CreateFuelingCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(
        CreateFuelingCommand request,
        CancellationToken cancellationToken = default)
    {
        var body = request.Body;
        var failures = new List<ValidationFailure>();

        if (body.KmAtFueling < 0)
            failures.Add(new ValidationFailure(nameof(body.KmAtFueling), "KmAtFueling is invalid."));
        if (body.Liters <= 0)
            failures.Add(new ValidationFailure(nameof(body.Liters), "Liters must be > 0."));
        if (body.TotalPrice < 0)
            failures.Add(new ValidationFailure(nameof(body.TotalPrice), "TotalPrice is invalid."));

        return new ValueTask<ValidationResult>(
            failures.Count == 0
                ? ValidationResult.Success
                : ValidationResult.FromFailures(failures));
    }
}
