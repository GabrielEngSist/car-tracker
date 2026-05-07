namespace Car.Tracker.Application.Mediator;

public interface IMediator
{
    /// <summary>Sends a request through the pipeline (validators + behaviors) and returns a non-throwing result.</summary>
    Task<ResponseValue<TResponse>> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}
