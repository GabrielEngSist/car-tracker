namespace Car.Tracker.Application.Mediator;

/// <summary>Factory methods for <see cref="HandlerResult{T}"/> (explicit <c>RequestOutcome.Ok</c> / <c>RequestOutcome.Fail</c> avoid generic <c>HandlerResult</c> ambiguity).</summary>
public static class RequestOutcome
{
    public static HandlerResult<T> Ok<T>(T value) => HandlerResult<T>.Ok(value);

    public static HandlerResult<T> Fail<T>(string code, string message, string? propertyName = null) =>
        HandlerResult<T>.Fail(code, message, propertyName);

    public static HandlerResult<T> Fail<T>(IReadOnlyList<FaultDetail> faults) => HandlerResult<T>.Fail(faults);

    public static HandlerResult<T> Fail<T>(params FaultDetail[] faults) => HandlerResult<T>.Fail(faults);
}

/// <summary>Result of a request handler — no exceptions for expected business/validation faults.</summary>
public readonly struct HandlerResult<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public IReadOnlyList<FaultDetail> Faults { get; }

    private HandlerResult(bool isSuccess, T? value, IReadOnlyList<FaultDetail> faults)
    {
        IsSuccess = isSuccess;
        Value = value;
        Faults = faults;
    }

    internal static HandlerResult<T> Ok(T value) => new(true, value, []);

    internal static HandlerResult<T> Fail(string code, string message, string? propertyName = null) =>
        new(false, default, [new FaultDetail(code, propertyName, message)]);

    internal static HandlerResult<T> Fail(IReadOnlyList<FaultDetail> faults) => new(false, default, faults);

    internal static HandlerResult<T> Fail(params FaultDetail[] faults) => Fail((IReadOnlyList<FaultDetail>)faults);
}
