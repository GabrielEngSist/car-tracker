namespace Car.Tracker.Application.Mediator;

/// <summary>
/// Discriminated result of <see cref="IMediator.SendAsync{TResponse}"/> —
/// success carries the handler response; failure carries validation errors (no exceptions for expected cases).
/// </summary>
public readonly struct ResponseValue<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public IReadOnlyList<ValidationFailure> Errors { get; }

    private ResponseValue(bool isSuccess, T? value, IReadOnlyList<ValidationFailure> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }

    public static ResponseValue<T> Success(T value) => new(true, value, []);

    public static ResponseValue<T> Failure(IReadOnlyList<ValidationFailure> errors) => new(false, default, errors);

    public static ResponseValue<T> Failure(params ValidationFailure[] errors) =>
        Failure((IReadOnlyList<ValidationFailure>)errors);
}
