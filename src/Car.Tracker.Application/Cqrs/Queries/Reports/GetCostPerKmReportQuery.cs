using Car.Tracker.Application.Mediator;
using Car.Tracker.Domain.Reports;

namespace Car.Tracker.Application.Cqrs.Queries.Reports;

public sealed record GetCostPerKmReportQuery(Guid CarId, string? Basis, string? Period, string? DistanceRef) : IRequest<CostPerKmReportDto?>;
