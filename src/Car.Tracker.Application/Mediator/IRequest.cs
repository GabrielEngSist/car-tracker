namespace Car.Tracker.Application.Mediator;

/// <summary>Marker for a request message (MediatR-style <c>IRequest&lt;TResponse&gt;</c>): implement on DTOs/commands/queries.</summary>
public interface IRequest<out TResponse> { }
