using Car.Tracker.Application.Cqrs.Commands.ExpenseEntries;
using Car.Tracker.Application.Cqrs.Queries.ExpenseEntries;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Entities;
using Car.Tracker.Tests.TestDoubles;

namespace Car.Tracker.Tests.Features.ExpenseEntries;

public sealed class ExpenseEntriesFeatureTests
{
    [Fact]
    public async Task CreateEntry_returns_failure_when_title_missing()
    {
        var db = new FakeTrackerPersistence();
        var car = new CarEntity { Model = "X", Year = 2020, CurrentKm = 0 };
        db.AddCar(car);

        var handler = new CreateExpenseEntryCommandHandler(db);
        var req = new CreateExpenseEntryRequest(
            Type: ExpenseEntryType.Service,
            Title: " ",
            Price: 10,
            SupplierBrand: null,
            ProductModel: null,
            PerformedAt: new DateOnly(2026, 1, 1),
            KmAtService: 10,
            Notes: null);

        var r = await handler.Handle(new CreateExpenseEntryCommand(car.Id, req), CancellationToken.None);

        Assert.True(r.IsFailure);
        Assert.Contains(r.Faults, f => f.Code == "TITLE_REQUIRED" && f.PropertyName == nameof(req.Title));
    }

    [Fact]
    public async Task EntriesByCar_returns_null_when_car_missing()
    {
        var handler = new GetCarExpenseEntriesQueryHandler(new FakeTrackerPersistence());

        var r = await handler.Handle(new GetCarExpenseEntriesQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(r.Value);
    }
}

