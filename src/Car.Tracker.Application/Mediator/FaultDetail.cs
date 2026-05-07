namespace Car.Tracker.Application.Mediator;

/// <summary>Structured fault: stable <see cref="Code"/>, optional field binding, human <see cref="Message"/>.</summary>
public readonly record struct FaultDetail(string? Code, string? PropertyName, string Message);

/// <summary>Output of <see cref="IValidator{TRequest}"/>.</summary>
public sealed class ValidationResult
{
    public static ValidationResult Ok { get; } = new([]);

    public IReadOnlyList<FaultDetail> Faults { get; }

    private ValidationResult(IReadOnlyList<FaultDetail> faults) => Faults = faults;

    public bool IsValid => Faults.Count == 0;

    public static ValidationResult FromFaults(params FaultDetail[] faults) =>
        faults.Length == 0 ? Ok : new ValidationResult([.. faults]);

    public static ValidationResult FromFaults(IReadOnlyList<FaultDetail> faults) =>
        faults.Count == 0 ? Ok : new ValidationResult(faults);
}
