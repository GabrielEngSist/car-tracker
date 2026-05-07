using Car.Tracker.Application.Mediator;

namespace Car.Tracker.Api;

internal static class MediatorHttp
{
    public static IResult ValidationProblem<T>(ResponseValue<T> response)
    {
        var unexpected = response.Faults.Any(f => string.Equals(f.Code, "UNEXPECTED", StringComparison.Ordinal));
        var statusCode = unexpected ? StatusCodes.Status500InternalServerError : StatusCodes.Status400BadRequest;
        var type = unexpected
            ? "https://tools.ietf.org/html/rfc9110#section-15.6.1"
            : "https://tools.ietf.org/html/rfc9110#section-15.5.1";
        var title = unexpected
            ? "An unexpected error occurred while processing the request."
            : "Request failed validation or could not be processed.";
        return Results.Json(
            new FaultPayload(type, title, response.Faults.Select(f => new FaultDto(f.Code, f.PropertyName, f.Message)).ToArray()),
            statusCode: statusCode);
    }

    private sealed record FaultPayload(string Type, string Title, FaultDto[] Faults);

    private sealed record FaultDto(string? Code, string? PropertyName, string Message);
}
