namespace Car.Tracker.Application.Mediator;

public interface IMediator
{
    /// <summary>
    /// Sends a request through the pipeline (validators + behaviors) and returns a non-throwing result.
    /// Reflection-free: callers provide the concrete request type (inferred by the compiler).
    /// </summary>
    Task<ResponseValue<TResponse>> SendAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;
}
