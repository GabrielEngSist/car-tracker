namespace Car.Tracker.Application.Mediator;

/// <summary>
/// MediatR-style pipeline behavior: outer behaviors run first; the last delegate invokes the handler (wrapped in <see cref="ResponseValue{T}"/>).
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<ResponseValue<TResponse>> Handle(
        TRequest request,
        Func<Task<ResponseValue<TResponse>>> next,
        CancellationToken cancellationToken);
}
