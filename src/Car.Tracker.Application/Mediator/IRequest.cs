namespace Car.Tracker.Application.Mediator;

/// <summary>Marker for a message that returns <typeparamref name="TResponse"/>.</summary>
public interface IRequest<out TResponse> { }
