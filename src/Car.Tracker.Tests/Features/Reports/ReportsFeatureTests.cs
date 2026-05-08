using Car.Tracker.Application.Cqrs.Queries.Reports;
using Car.Tracker.Domain.Entities;
using Car.Tracker.Tests.TestDoubles;

namespace Car.Tracker.Tests.Features.Reports;

public sealed class ReportsFeatureTests
{
    [Fact]
    public async Task CostPerKm_report_returns_failure_for_invalid_basis()
    {
        var handler = new GetCostPerKmReportQueryHandler(new FakeTrackerPersistence());

        var r = await handler.Handle(new GetCostPerKmReportQuery(Guid.NewGuid(), Basis: "nope", Period: null, DistanceRef: null), CancellationToken.None);

        Assert.True(r.IsFailure);
        Assert.Contains(r.Faults, f => f.Code == "BASIS_INVALID" && f.PropertyName == nameof(GetCostPerKmReportQuery.Basis));
    }

    [Fact]
    public async Task CostPerKm_report_returns_null_when_car_missing()
    {
        var handler = new GetCostPerKmReportQueryHandler(new FakeTrackerPersistence());

        var r = await handler.Handle(new GetCostPerKmReportQuery(Guid.NewGuid(), Basis: "lifetime", Period: null, DistanceRef: null), CancellationToken.None);

        Assert.Null(r.Value);
    }

    [Fact]
    public async Task FuelFullTank_report_returns_null_when_car_missing()
    {
        var handler = new GetFuelFullTankReportQueryHandler(new FakeTrackerPersistence());

        var r = await handler.Handle(new GetFuelFullTankReportQuery(Guid.NewGuid(), Basis: "lifetime", Period: null), CancellationToken.None);

        Assert.Null(r.Value);
    }

    [Fact]
    public async Task Reports_compute_when_data_present()
    {
        var db = new FakeTrackerPersistence();
        var car = new CarEntity { Model = "X", Year = 2020, CurrentKm = 2000 };
        db.AddCar(car);

        db.AddFueling(new FuelingEntry { CarId = car.Id, PerformedAt = new DateOnly(2026, 1, 1), KmAtFueling = 1000, Liters = 40, TotalPrice = 200, IsFullTank = true });
        db.AddFueling(new FuelingEntry { CarId = car.Id, PerformedAt = new DateOnly(2026, 2, 1), KmAtFueling = 1500, Liters = 35, TotalPrice = 180, IsFullTank = true });

        db.AddExpenseEntry(new ExpenseEntry
        {
            CarId = car.Id,
            Type = ExpenseEntryType.Service,
            Title = "Oil",
            Price = 120,
            PerformedAt = new DateOnly(2026, 2, 10),
            KmAtService = 1600,
        });

        var costHandler = new GetCostPerKmReportQueryHandler(db);
        var fuelHandler = new GetFuelFullTankReportQueryHandler(db);

        var cost = await costHandler.Handle(new GetCostPerKmReportQuery(car.Id, Basis: "lifetime", Period: null, DistanceRef: "total"), CancellationToken.None);
        var fuel = await fuelHandler.Handle(new GetFuelFullTankReportQuery(car.Id, Basis: "lifetime", Period: null), CancellationToken.None);

        Assert.NotNull(cost.Value);
        Assert.NotNull(fuel.Value);
    }
}

