using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Car.Tracker.Application.Mediator;

/// <summary>Resolves <see cref="IRequestHandler{TRequest,TResponse}"/> from DI and dispatches (single-handler-per-request).</summary>
public sealed class DefaultMediator(IServiceProvider serviceProvider) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = serviceProvider.GetRequiredService(handlerType);

        var handle = handler.GetType().GetMethod(
            "Handle",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            [requestType, typeof(CancellationToken)],
            null);
        if (handle is null)
            throw new InvalidOperationException($"Handle method not found on {handler.GetType().Name}.");

        var task = (Task)handle.Invoke(handler, [request, cancellationToken])!;
        return CastTask<TResponse>(task);
    }

    private static async Task<TResponse> CastTask<TResponse>(Task task)
    {
        await task.ConfigureAwait(false);
        var taskType = task.GetType();
        if (!taskType.IsGenericType || taskType.GetGenericTypeDefinition() != typeof(Task<>))
            throw new InvalidOperationException("Handler must return Task<TResponse>.");

        return (TResponse)taskType.GetProperty(nameof(Task<object>.Result))!.GetValue(task)!;
    }
}
