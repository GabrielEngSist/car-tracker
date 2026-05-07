using Car.Tracker.Application.Mediator;

namespace Car.Tracker.Api;

internal static class MediatorHttp
{
    public static IResult ValidationProblem<T>(ResponseValue<T> response)
    {
        var dict = ToErrorDictionary(response.Errors);
        return Results.ValidationProblem(dict);
    }

    private static Dictionary<string, string[]> ToErrorDictionary(IReadOnlyList<ValidationFailure> errors)
    {
        if (errors.Count == 0)
            return new Dictionary<string, string[]> { ["_error"] = ["Validation failed."] };

        return errors
            .GroupBy(e => string.IsNullOrEmpty(e.PropertyName) ? "_error" : e.PropertyName!)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Message).ToArray());
    }
}
