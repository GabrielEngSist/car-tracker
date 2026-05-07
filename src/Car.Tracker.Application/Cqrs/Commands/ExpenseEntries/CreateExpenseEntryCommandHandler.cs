using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Entities;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.ExpenseEntries;

public sealed class CreateExpenseEntryCommandHandler(ITrackerPersistence db) : IRequestHandler<CreateExpenseEntryCommand, ExpenseEntryDto?>
{
    public async Task<HandlerResult<ExpenseEntryDto?>> Handle(CreateExpenseEntryCommand request, CancellationToken cancellationToken)
    {
        var car = await db.GetCarByIdTrackedAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (car is null) return RequestOutcome.Ok<ExpenseEntryDto?>(null);

        var body = request.Body;
        if (string.IsNullOrWhiteSpace(body.Title))
            return RequestOutcome.Fail<ExpenseEntryDto?>("TITLE_REQUIRED", "Title is required.", nameof(body.Title));
        if (body.Price < 0)
            return RequestOutcome.Fail<ExpenseEntryDto?>("PRICE_INVALID", "Price is invalid.", nameof(body.Price));
        if (body.KmAtService < 0)
            return RequestOutcome.Fail<ExpenseEntryDto?>("KM_AT_SERVICE_INVALID", "KmAtService is invalid.", nameof(body.KmAtService));

        var entry = new ExpenseEntry
        {
            CarId = request.CarId,
            Type = body.Type,
            Title = body.Title.Trim(),
            Price = body.Price,
            SupplierBrand = string.IsNullOrWhiteSpace(body.SupplierBrand) ? null : body.SupplierBrand.Trim(),
            ProductModel = string.IsNullOrWhiteSpace(body.ProductModel) ? null : body.ProductModel.Trim(),
            PerformedAt = body.PerformedAt,
            KmAtService = body.KmAtService,
            Notes = string.IsNullOrWhiteSpace(body.Notes) ? null : body.Notes.Trim(),
        };

        db.AddExpenseEntry(entry);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return RequestOutcome.Ok<ExpenseEntryDto?>(new ExpenseEntryDto(
            entry.Id, entry.CarId, entry.Type, entry.Title, entry.Price, entry.SupplierBrand, entry.ProductModel, entry.PerformedAt, entry.KmAtService, entry.Notes));
    }
}
