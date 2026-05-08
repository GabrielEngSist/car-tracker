using Car.Tracker.Application.Cqrs.Commands.Maintenance;
using Car.Tracker.Application.Cqrs.Queries.Maintenance;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Entities;
using Car.Tracker.Tests.TestDoubles;

namespace Car.Tracker.Tests.Features.Maintenance;

public sealed class MaintenanceFeatureTests
{
    [Fact]
    public async Task CreateMaintenancePlan_requires_title()
    {
        var db = new FakeTrackerPersistence();
        var car = new CarEntity { Model = "X", Year = 2020, CurrentKm = 0 };
        db.AddCar(car);

        var handler = new CreateMaintenancePlanItemCommandHandler(db);
        var req = new CreateMaintenancePlanItemRequest(Title: " ", DueKmInterval: 1000, DueTimeIntervalDays: null, Active: true);

        var r = await handler.Handle(new CreateMaintenancePlanItemCommand(car.Id, req), CancellationToken.None);

        Assert.True(r.IsFailure);
        Assert.Contains(r.Faults, f => f.Code == "TITLE_REQUIRED" && f.PropertyName == nameof(req.Title));
    }

    [Fact]
    public async Task GetMaintenancePlans_returns_null_when_car_missing()
    {
        var handler = new GetMaintenancePlansQueryHandler(new FakeTrackerPersistence());

        var r = await handler.Handle(new GetMaintenancePlansQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(r.Value);
    }

    [Fact]
    public async Task GetMaintenanceStatus_returns_null_when_car_missing()
    {
        var handler = new GetMaintenanceStatusQueryHandler(new FakeTrackerPersistence());

        var r = await handler.Handle(new GetMaintenanceStatusQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(r.Value);
    }
}

