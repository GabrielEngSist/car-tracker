using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;

namespace Car.Tracker.Application.Cqrs.Commands.ExpenseEntries;

public sealed record CreateExpenseEntryCommand(Guid CarId, CreateExpenseEntryRequest Body) : IRequest<ExpenseEntryDto?>;
