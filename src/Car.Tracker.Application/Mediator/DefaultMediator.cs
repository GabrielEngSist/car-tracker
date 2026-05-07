using Microsoft.Extensions.DependencyInjection;

namespace Car.Tracker.Application.Mediator;

/// <summary>
/// Resolves handlers and <see cref="IPipelineBehavior{TRequest,TResponse}"/> (MediatR-style pipeline).
/// Maps handler faults to <see cref="ResponseValue{T}"/>; unexpected exceptions become a single fault with code <c>UNEXPECTED</c>.
/// </summary>
public sealed class DefaultMediator(IServiceProvider serviceProvider) : IMediator
{
    public Task<ResponseValue<TResponse>> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.DispatchAsync(this, cancellationToken);
    }

    public async Task<ResponseValue<TResponse>> SendAsync<TRequest, TResponse>(
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
            var outcome = await handler.Handle(request, cancellationToken).ConfigureAwait(false);
            if (outcome.IsFailure)
                return ResponseValue<TResponse>.Failure(outcome.Faults);

            return ResponseValue<TResponse>.Success(outcome.Value!);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ResponseValue<TResponse>.Failure(
                new FaultDetail(
                    "UNEXPECTED",
                    null,
                    string.IsNullOrWhiteSpace(ex.Message) ? "An unexpected error occurred." : ex.Message));
        }
    }
}
