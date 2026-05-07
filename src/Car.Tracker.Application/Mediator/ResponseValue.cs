namespace Car.Tracker.Application.Mediator;

/// <summary>
/// Discriminated result of <see cref="IMediator.SendAsync{TResponse}"/> —
/// success carries the handler response; failure carries structured faults (no exceptions for expected cases).
/// </summary>
public readonly struct ResponseValue<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public IReadOnlyList<FaultDetail> Faults { get; }

    private ResponseValue(bool isSuccess, T? value, IReadOnlyList<FaultDetail> faults)
    {
        IsSuccess = isSuccess;
        Value = value;
        Faults = faults;
    }

    public static ResponseValue<T> Success(T value) => new(true, value, []);

    public static ResponseValue<T> Failure(IReadOnlyList<FaultDetail> faults) => new(false, default, faults);

    public static ResponseValue<T> Failure(params FaultDetail[] faults) =>
        Failure((IReadOnlyList<FaultDetail>)faults);
}
