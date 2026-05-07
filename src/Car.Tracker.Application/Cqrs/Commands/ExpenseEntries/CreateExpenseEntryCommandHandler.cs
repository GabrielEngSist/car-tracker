using Car.Tracker.Application.Common;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Entities;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.ExpenseEntries;

public sealed class CreateExpenseEntryCommandHandler(ITrackerPersistence db) : IRequestHandler<CreateExpenseEntryCommand, ExpenseEntryDto?>
{
    public async Task<ExpenseEntryDto?> Handle(CreateExpenseEntryCommand request, CancellationToken cancellationToken)
    {
        var car = await db.GetCarByIdTrackedAsync(request.CarId, cancellationToken).ConfigureAwait(false);
        if (car is null) return null;

        var body = request.Body;
        if (string.IsNullOrWhiteSpace(body.Title)) throw new ValidationException("Title is required.");
        if (body.Price < 0) throw new ValidationException("Price is invalid.");
        if (body.KmAtService < 0) throw new ValidationException("KmAtService is invalid.");

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

        return new ExpenseEntryDto(
            entry.Id, entry.CarId, entry.Type, entry.Title, entry.Price, entry.SupplierBrand, entry.ProductModel, entry.PerformedAt, entry.KmAtService, entry.Notes);
    }
}
