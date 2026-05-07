namespace Car.Tracker.Application.Mediator;

/// <summary>Request message (MediatR-style <c>IRequest&lt;TResponse&gt;</c>).</summary>
public interface IRequest<TResponse>
{
    /// <summary>
    /// Reflection-free dispatch: the request knows its own compile-time type, so it can call the typed mediator overload.
    /// </summary>
    Task<ResponseValue<TResponse>> DispatchAsync(IMediator mediator, CancellationToken cancellationToken = default);
}
