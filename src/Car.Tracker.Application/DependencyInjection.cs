using Car.Tracker.Application.Cqrs.Commands.Cars;
using Car.Tracker.Application.Cqrs.Commands.ExpenseEntries;
using Car.Tracker.Application.Cqrs.Commands.Fuelings;
using Car.Tracker.Application.Cqrs.Commands.Maintenance;
using Car.Tracker.Application.Cqrs.Queries.Cars;
using Car.Tracker.Application.Cqrs.Queries.ExpenseEntries;
using Car.Tracker.Application.Cqrs.Queries.Fuelings;
using Car.Tracker.Application.Cqrs.Queries.Maintenance;
using Car.Tracker.Application.Cqrs.Queries.Reports;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Reports;
using Microsoft.Extensions.DependencyInjection;

namespace Car.Tracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMediator, DefaultMediator>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

        services.AddScoped<IValidator<CreateFuelingCommand>, CreateFuelingCommandValidator>();

        services.AddScoped<IRequestHandler<GetCarsQuery, IReadOnlyList<CarDto>>, GetCarsQueryHandler>();
        services.AddScoped<IRequestHandler<GetCarByIdQuery, CarDto?>, GetCarByIdQueryHandler>();
        services.AddScoped<IRequestHandler<GetCarRegistryQuery, CarRegistryDto?>, GetCarRegistryQueryHandler>();
        services.AddScoped<IRequestHandler<CreateCarCommand, CreateCarOutcome>, CreateCarCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateCarCommand, CarDto?>, UpdateCarCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteCarCommand, bool>, DeleteCarCommandHandler>();

        services.AddScoped<IRequestHandler<GetFuelFullTankReportQuery, FuelFullTankEfficiencyReportDto?>, GetFuelFullTankReportQueryHandler>();
        services.AddScoped<IRequestHandler<GetCostPerKmReportQuery, CostPerKmReportDto?>, GetCostPerKmReportQueryHandler>();

        services.AddScoped<IRequestHandler<GetCarFuelingsQuery, IReadOnlyList<FuelingEntryDto>?>, GetCarFuelingsQueryHandler>();
        services.AddScoped<IRequestHandler<GetAllFuelingsQuery, IReadOnlyList<FuelingEntryDto>>, GetAllFuelingsQueryHandler>();
        services.AddScoped<IRequestHandler<CreateFuelingCommand, FuelingEntryDto?>, CreateFuelingCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateFuelingCommand, FuelingEntryDto?>, UpdateFuelingCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteFuelingCommand, bool>, DeleteFuelingCommandHandler>();

        services.AddScoped<IRequestHandler<GetCarExpenseEntriesQuery, IReadOnlyList<ExpenseEntryDto>?>, GetCarExpenseEntriesQueryHandler>();
        services.AddScoped<IRequestHandler<CreateExpenseEntryCommand, ExpenseEntryDto?>, CreateExpenseEntryCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateExpenseEntryCommand, ExpenseEntryDto?>, UpdateExpenseEntryCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteExpenseEntryCommand, bool>, DeleteExpenseEntryCommandHandler>();

        services.AddScoped<IRequestHandler<GetMaintenancePlansQuery, IReadOnlyList<MaintenancePlanItemDto>?>, GetMaintenancePlansQueryHandler>();
        services.AddScoped<IRequestHandler<GetMaintenanceStatusQuery, IReadOnlyList<MaintenanceStatusDto>?>, GetMaintenanceStatusQueryHandler>();
        services.AddScoped<IRequestHandler<CreateMaintenancePlanItemCommand, MaintenancePlanItemDto?>, CreateMaintenancePlanItemCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateMaintenancePlanItemCommand, MaintenancePlanItemDto?>, UpdateMaintenancePlanItemCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteMaintenancePlanItemCommand, bool>, DeleteMaintenancePlanItemCommandHandler>();

        return services;
    }
}
