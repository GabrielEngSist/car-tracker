using System.Reflection;
using Car.Tracker.Application.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Car.Tracker.Application.Mediator;

/// <summary>
/// Resolves handlers and <see cref="IPipelineBehavior{TRequest,TResponse}"/> like MediatR; maps <see cref="ValidationException"/> to faulted <see cref="ResponseValue{T}"/>.
/// </summary>
public sealed class DefaultMediator(IServiceProvider serviceProvider) : IMediator
{
    private static readonly MethodInfo DispatchAsyncImplInfo =
        typeof(DefaultMediator).GetMethod(nameof(DispatchAsyncImpl), BindingFlags.Instance | BindingFlags.NonPublic)!;

    public Task<ResponseValue<TResponse>> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var requestType = request.GetType();
        var closed = DispatchAsyncImplInfo.MakeGenericMethod(requestType, typeof(TResponse));
        return (Task<ResponseValue<TResponse>>)closed.Invoke(this, [request, cancellationToken])!;
    }

    private async Task<ResponseValue<TResponse>> DispatchAsyncImpl<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().ToList();

        Func<Task<ResponseValue<TResponse>>> next = () => InvokeHandler(handler, request, cancellationToken);

        for (var i = behaviors.Count - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var inner = next;
            next = () => behavior.Handle(request, inner, cancellationToken);
        }

        return await next().ConfigureAwait(false);
    }

    private static async Task<ResponseValue<TResponse>> InvokeHandler<TRequest, TResponse>(
        IRequestHandler<TRequest, TResponse> handler,
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        try
        {
            var result = await handler.Handle(request, cancellationToken).ConfigureAwait(false);
            return ResponseValue<TResponse>.Success(result);
        }
        catch (ValidationException ex)
        {
            return ResponseValue<TResponse>.Failure([new ValidationFailure(null, ex.Message)]);
        }
    }
}
