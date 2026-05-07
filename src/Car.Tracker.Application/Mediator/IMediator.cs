namespace Car.Tracker.Application.Mediator;

public interface IMediator
{
    /// <summary>Sends a request through the pipeline (validators + behaviors) and returns a non-throwing result.</summary>
    Task<ResponseValue<TResponse>> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request through the pipeline when the concrete <typeparamref name="TRequest"/> is known at compile time.
    /// This overload exists to enable reflection-free dispatch (double-dispatch from the request).
    /// </summary>
    Task<ResponseValue<TResponse>> SendAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;
}
