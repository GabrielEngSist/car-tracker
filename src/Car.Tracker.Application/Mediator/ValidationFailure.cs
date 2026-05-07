namespace Car.Tracker.Application.Mediator;

/// <summary>Single validation message, optionally bound to a property (MediatR/FluentValidation-style).</summary>
public readonly record struct ValidationFailure(string? PropertyName, string Message);

/// <summary>Output of <see cref="IValidator{TRequest}"/>.</summary>
public sealed class ValidationResult
{
    public static ValidationResult Success { get; } = new([]);
    public IReadOnlyList<ValidationFailure> Errors { get; }

    private ValidationResult(IReadOnlyList<ValidationFailure> errors) => Errors = errors;

    public bool IsValid => Errors.Count == 0;

    public static ValidationResult FromFailures(params ValidationFailure[] errors) =>
        errors.Length == 0 ? Success : new ValidationResult([.. errors]);

    public static ValidationResult FromFailures(IReadOnlyList<ValidationFailure> errors) =>
        errors.Count == 0 ? Success : new ValidationResult(errors);
}
