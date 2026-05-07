using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Application.Cqrs.Queries.ExpenseEntries;

public sealed record GetCarExpenseEntriesQuery(Guid CarId) : Request<GetCarExpenseEntriesQuery, IReadOnlyList<ExpenseEntryDto>?>;
