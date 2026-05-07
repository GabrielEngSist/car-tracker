using Car.Tracker.Application.Mediator;
using Car.Tracker.Domain.Reports;

namespace Car.Tracker.Application.Cqrs.Queries.Reports;

public sealed record GetFuelFullTankReportQuery(Guid CarId, string? Basis, string? Period) : IRequest<FuelFullTankEfficiencyReportDto?>;
