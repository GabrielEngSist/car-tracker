using Car.Tracker.Application.Common;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.ExpenseEntries;

public sealed class UpdateExpenseEntryCommandHandler(ITrackerPersistence db) : IRequestHandler<UpdateExpenseEntryCommand, ExpenseEntryDto?>
{
    public async Task<ExpenseEntryDto?> Handle(UpdateExpenseEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await db.GetExpenseEntryByCarAndIdTrackedAsync(request.CarId, request.EntryId, cancellationToken).ConfigureAwait(false);
        if (entry is null) return null;

        var body = request.Body;
        if (body.Type is not null) entry.Type = body.Type.Value;
        if (body.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(body.Title)) throw new ValidationException("Title cannot be empty.");
            entry.Title = body.Title.Trim();
        }
        if (body.Price is not null)
        {
            if (body.Price.Value < 0) throw new ValidationException("Price is invalid.");
            entry.Price = body.Price.Value;
        }
        if (body.SupplierBrand is not null)
            entry.SupplierBrand = string.IsNullOrWhiteSpace(body.SupplierBrand) ? null : body.SupplierBrand.Trim();
        if (body.ProductModel is not null)
            entry.ProductModel = string.IsNullOrWhiteSpace(body.ProductModel) ? null : body.ProductModel.Trim();
        if (body.PerformedAt is not null) entry.PerformedAt = body.PerformedAt.Value;
        if (body.KmAtService is not null)
        {
            if (body.KmAtService.Value < 0) throw new ValidationException("KmAtService is invalid.");
            entry.KmAtService = body.KmAtService.Value;
        }
        if (body.Notes is not null)
            entry.Notes = string.IsNullOrWhiteSpace(body.Notes) ? null : body.Notes.Trim();

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new ExpenseEntryDto(
            entry.Id, entry.CarId, entry.Type, entry.Title, entry.Price, entry.SupplierBrand, entry.ProductModel, entry.PerformedAt, entry.KmAtService, entry.Notes);
    }
}
