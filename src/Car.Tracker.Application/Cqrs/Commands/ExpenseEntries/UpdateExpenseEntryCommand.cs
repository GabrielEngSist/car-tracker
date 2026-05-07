using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Application.Cqrs.Commands.ExpenseEntries;

public sealed record UpdateExpenseEntryCommand(Guid CarId, Guid EntryId, UpdateExpenseEntryRequest Body) : Request<UpdateExpenseEntryCommand, ExpenseEntryDto?>;
