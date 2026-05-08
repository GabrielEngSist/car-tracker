using Car.Tracker.Application.Cqrs.Commands.Cars;
using Car.Tracker.Application.Cqrs.Queries.Cars;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Entities;
using Car.Tracker.Tests.TestDoubles;

namespace Car.Tracker.Tests.Features.Cars;

public sealed class CarQueriesAndMutationsTests
{
    [Fact]
    public async Task GetCars_returns_cars_ordered_by_created_desc()
    {
        var db = new FakeTrackerPersistence();
        db.AddCar(new CarEntity { Model = "A", Year = 2000, CurrentKm = 0, CreatedAt = DateTimeOffset.UtcNow.AddDays(-1), UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1) });
        db.AddCar(new CarEntity { Model = "B", Year = 2000, CurrentKm = 0, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });

        var handler = new GetCarsQueryHandler(db);
        var r = await handler.Handle(new GetCarsQuery(), CancellationToken.None);

        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Value);
        Assert.Equal(["B", "A"], r.Value!.Select(x => x.Model).ToArray());
    }

    [Fact]
    public async Task GetCarById_returns_null_when_missing()
    {
        var handler = new GetCarByIdQueryHandler(new FakeTrackerPersistence());

        var r = await handler.Handle(new GetCarByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(r.IsSuccess);
        Assert.Null(r.Value);
    }

    [Fact]
    public async Task UpdateCar_returns_failure_fault_for_invalid_year()
    {
        var db = new FakeTrackerPersistence();
        var car = new CarEntity { Model = "X", Year = 2020, CurrentKm = 10 };
        db.AddCar(car);

        var handler = new UpdateCarCommandHandler(db);
        var req = new UpdateCarRequest(Model: null, Year: 1800, CurrentKm: null, Name: null, Placa: null);

        var r = await handler.Handle(new UpdateCarCommand(car.Id, req), CancellationToken.None);

        Assert.True(r.IsFailure);
        Assert.Contains(r.Faults, f => f.Code == "YEAR_INVALID" && f.PropertyName == nameof(req.Year));
    }

    [Fact]
    public async Task DeleteCar_returns_false_when_missing()
    {
        var handler = new DeleteCarCommandHandler(new FakeTrackerPersistence());

        var r = await handler.Handle(new DeleteCarCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(r.IsSuccess);
        Assert.False(r.Value);
    }

    [Fact]
    public async Task Car_registry_query_returns_null_when_missing()
    {
        var handler = new GetCarRegistryQueryHandler(new FakeTrackerPersistence());

        var r = await handler.Handle(new GetCarRegistryQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(r.IsSuccess);
        Assert.Null(r.Value);
    }
}

