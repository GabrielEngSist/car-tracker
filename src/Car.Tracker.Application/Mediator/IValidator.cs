namespace Car.Tracker.Application.Mediator;

/// <summary>Optional per-request validator run in the pipeline before the handler (register <c>IValidator{TRequest}</c> in DI).</summary>
public interface IValidator<in TRequest>
{
    ValueTask<ValidationResult> ValidateAsync(TRequest request, CancellationToken cancellationToken = default);
}
