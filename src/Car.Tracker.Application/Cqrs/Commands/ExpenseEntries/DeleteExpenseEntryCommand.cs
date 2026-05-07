using Car.Tracker.Application.Mediator;

namespace Car.Tracker.Application.Cqrs.Commands.ExpenseEntries;

public sealed record DeleteExpenseEntryCommand(Guid CarId, Guid EntryId) : IRequest<bool>;
