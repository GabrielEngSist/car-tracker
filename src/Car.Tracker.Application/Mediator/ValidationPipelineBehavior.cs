using Microsoft.Extensions.DependencyInjection;

namespace Car.Tracker.Application.Mediator;

/// <summary>Runs <see cref="IValidator{TRequest}"/> when registered; short-circuits with <see cref="ResponseValue{T}.Failure"/>.</summary>
public sealed class ValidationPipelineBehavior<TRequest, TResponse>(IServiceProvider serviceProvider)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<ResponseValue<TResponse>> Handle(
        TRequest request,
        Func<Task<ResponseValue<TResponse>>> next,
        CancellationToken cancellationToken)
    {
        var validator = serviceProvider.GetService<IValidator<TRequest>>();
        if (validator is not null)
        {
            var result = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!result.IsValid)
                return ResponseValue<TResponse>.Failure(result.Errors);
        }

        return await next().ConfigureAwait(false);
    }
}
