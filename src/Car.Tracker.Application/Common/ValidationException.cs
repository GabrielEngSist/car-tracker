namespace Car.Tracker.Application.Common;

/// <summary>Application-level validation error mapped to HTTP 400 by the API.</summary>
public sealed class ValidationException(string message) : Exception(message);
