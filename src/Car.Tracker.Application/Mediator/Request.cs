namespace Car.Tracker.Application.Mediator;

/// <summary>
/// Base request that implements reflection-free double-dispatch into <see cref="IMediator"/>.
/// </summary>
public abstract record Request<TSelf, TResponse> : IRequest<TResponse>
    where TSelf : Request<TSelf, TResponse>
{
    public Task<ResponseValue<TResponse>> DispatchAsync(IMediator mediator, CancellationToken cancellationToken = default) =>
        mediator.SendAsync<TSelf, TResponse>((TSelf)this, cancellationToken);
}

