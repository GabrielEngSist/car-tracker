using Car.Tracker.Application.Cqrs.Commands.Fuelings;
using Car.Tracker.Application.Cqrs.Queries.Fuelings;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Entities;
using Car.Tracker.Tests.TestDoubles;

namespace Car.Tracker.Tests.Features.Fuelings;

public sealed class FuelingFeatureTests
{
    [Fact]
    public async Task CreateFueling_returns_null_when_car_missing()
    {
        var handler = new CreateFuelingCommandHandler(new FakeTrackerPersistence());
        var req = new CreateFuelingEntryRequest(
            PerformedAt: new DateOnly(2026, 1, 1),
            KmAtFueling: 10,
            Liters: 10,
            TotalPrice: 50,
            FuelType: null,
            IsFullTank: null,
            StationName: null,
            Notes: null);

        var r = await handler.Handle(new CreateFuelingCommand(Guid.NewGuid(), req), CancellationToken.None);

        Assert.True(r.IsSuccess);
        Assert.Null(r.Value);
    }

    [Fact]
    public async Task CreateFueling_updates_car_current_km_when_higher()
    {
        var db = new FakeTrackerPersistence();
        var car = new CarEntity { Model = "X", Year = 2020, CurrentKm = 100 };
        db.AddCar(car);

        var handler = new CreateFuelingCommandHandler(db);
        var req = new CreateFuelingEntryRequest(
            PerformedAt: new DateOnly(2026, 1, 1),
            KmAtFueling: 150,
            Liters: 10,
            TotalPrice: 50,
            FuelType: FuelType.Gasolina,
            IsFullTank: true,
            StationName: "  Shell ",
            Notes: "  ok ");

        var r = await handler.Handle(new CreateFuelingCommand(car.Id, req), CancellationToken.None);

        Assert.NotNull(r.Value);
        Assert.Single(db.Fuelings);
        Assert.Equal(150, db.Cars.Single().CurrentKm);
        Assert.Equal("Shell", r.Value!.StationName);
        Assert.Equal("ok", r.Value.Notes);
    }

    [Fact]
    public async Task CreateFueling_validator_emits_faults_per_fault_envelope_conventions()
    {
        var validator = new CreateFuelingCommandValidator();
        var req = new CreateFuelingEntryRequest(
            PerformedAt: new DateOnly(2026, 1, 1),
            KmAtFueling: -1,
            Liters: 0,
            TotalPrice: -10,
            FuelType: null,
            IsFullTank: null,
            StationName: null,
            Notes: null);

        var result = await validator.ValidateAsync(new CreateFuelingCommand(Guid.NewGuid(), req), CancellationToken.None);

        Assert.False(result.IsValid);
        var codes = result.Faults.Select(f => f.Code).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("INVALID_KM", codes);
        Assert.Contains("INVALID_LITERS", codes);
        Assert.Contains("INVALID_TOTAL_PRICE", codes);
    }

    [Fact]
    public async Task GetFuelingsByCar_returns_null_when_car_missing()
    {
        var handler = new GetCarFuelingsQueryHandler(new FakeTrackerPersistence());

        var r = await handler.Handle(new GetCarFuelingsQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(r.Value);
    }

    [Fact]
    public async Task GetAllFuelings_returns_items_ordered_desc()
    {
        var db = new FakeTrackerPersistence();
        var car = new CarEntity { Model = "X", Year = 2020, CurrentKm = 0 };
        db.AddCar(car);
        db.AddFueling(new FuelingEntry { CarId = car.Id, PerformedAt = new DateOnly(2026, 1, 1), KmAtFueling = 1, Liters = 1, TotalPrice = 1 });
        db.AddFueling(new FuelingEntry { CarId = car.Id, PerformedAt = new DateOnly(2026, 2, 1), KmAtFueling = 2, Liters = 1, TotalPrice = 1 });

        var handler = new GetAllFuelingsQueryHandler(db);
        var r = await handler.Handle(new GetAllFuelingsQuery(), CancellationToken.None);

        Assert.NotNull(r.Value);
        Assert.Equal(
            [new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1)],
            r.Value!.Select(x => x.PerformedAt).ToArray());
    }
}

