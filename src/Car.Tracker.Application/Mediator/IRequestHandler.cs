namespace Car.Tracker.Application.Mediator;

public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<HandlerResult<TResponse>> Handle(TRequest request, CancellationToken cancellationToken);
}
