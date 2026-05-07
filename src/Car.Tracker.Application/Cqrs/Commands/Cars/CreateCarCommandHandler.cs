using System.Net.Http;
using Car.Tracker.Application.Mediator;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Entities;
using Car.Tracker.Domain.Integration;
using Car.Tracker.Domain.Plates;
using Car.Tracker.Domain.Ports.Integration;
using Car.Tracker.Domain.Ports.Persistence;

namespace Car.Tracker.Application.Cqrs.Commands.Cars;

public sealed class CreateCarCommandHandler(
    ITrackerPersistence db,
    IConsultarPlacaPort consultarPlaca,
    IConsultarPrecoFipePort consultarPrecoFipe) : IRequestHandler<CreateCarCommand, CreateCarOutcome>
{
    public async Task<HandlerResult<CreateCarOutcome>> Handle(CreateCarCommand request, CancellationToken cancellationToken)
    {
        var r = request.Request;
        if (r.CurrentKm < 0)
            return RequestOutcome.Ok(new CreateCarOutcome(CreateCarStatus.BadRequest, null, "CurrentKm is invalid."));

        if (r.AutoBuscarDados)
        {
            if (string.IsNullOrWhiteSpace(r.Placa))
                return RequestOutcome.Ok(new CreateCarOutcome(CreateCarStatus.BadRequest, null, "Placa is required for automatic registration."));

            var placaNorm = PlacaBrasil.Normalizar(r.Placa);
            if (!PlacaBrasil.EhValida(placaNorm))
                return RequestOutcome.Ok(new CreateCarOutcome(CreateCarStatus.BadRequest, null, "Invalid plate format (use Mercosul or old Brazilian format)."));

            try
            {
                var rPlaca = await consultarPlaca.ConsultarPorPlacaAsync(placaNorm, cancellationToken).ConfigureAwait(false);
                var rFipe = await consultarPrecoFipe.ConsultarPorPlacaAsync(placaNorm, cancellationToken).ConfigureAwait(false);

                if (rPlaca is null || !string.Equals(rPlaca.Status, "ok", StringComparison.OrdinalIgnoreCase))
                    return RequestOutcome.Ok(new CreateCarOutcome(CreateCarStatus.BadRequest, null, $"consultarPlaca failed: {rPlaca?.Mensagem ?? "empty response"}"));

                if (rFipe is null || !string.Equals(rFipe.Status, "ok", StringComparison.OrdinalIgnoreCase))
                    return RequestOutcome.Ok(new CreateCarOutcome(CreateCarStatus.BadRequest, null, $"consultarPrecoFipe failed: {rFipe?.Mensagem ?? "empty response"}"));

                var car = new CarEntity { Model = "temp", Year = 2000, CurrentKm = r.CurrentKm };
                ConsultarPlacaMapper.PreencherCarro(car, rPlaca, r.CurrentKm, r.Name, placaNorm);
                car.ConsultaPlaca = ConsultarPlacaMapper.ToConsultaPlaca(car.Id, rPlaca);
                car.ConsultaPrecoFipe = ConsultarPrecoFipeMapper.ToConsultaPrecoFipe(car.Id, rFipe);

                db.AddCar(car);
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var dto = new CarDto(car.Id, car.Model, car.Year, car.CurrentKm, car.Name, car.Placa, car.CreatedAt, car.UpdatedAt);
                return RequestOutcome.Ok(new CreateCarOutcome(CreateCarStatus.Created, dto, null));
            }
            catch (HttpRequestException ex)
            {
                return RequestOutcome.Ok(new CreateCarOutcome(CreateCarStatus.BadGateway, null, ex.Message));
            }
        }

        if (string.IsNullOrWhiteSpace(r.Model))
            return RequestOutcome.Ok(new CreateCarOutcome(CreateCarStatus.BadRequest, null, "Model is required."));
        if (r.Year is null or < 1900 or > 3000)
            return RequestOutcome.Ok(new CreateCarOutcome(CreateCarStatus.BadRequest, null, "Year is invalid."));

        string? placaManual = null;
        if (!string.IsNullOrWhiteSpace(r.Placa))
        {
            placaManual = PlacaBrasil.Normalizar(r.Placa);
            if (!PlacaBrasil.EhValida(placaManual))
                return RequestOutcome.Ok(new CreateCarOutcome(CreateCarStatus.BadRequest, null, "Invalid plate format."));
        }

        var carManual = new CarEntity
        {
            Model = r.Model.Trim(),
            Year = r.Year.Value,
            CurrentKm = r.CurrentKm,
            Name = string.IsNullOrWhiteSpace(r.Name) ? null : r.Name.Trim(),
            Placa = placaManual,
        };

        db.AddCar(carManual);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var created = new CarDto(carManual.Id, carManual.Model, carManual.Year, carManual.CurrentKm, carManual.Name, carManual.Placa, carManual.CreatedAt, carManual.UpdatedAt);
        return RequestOutcome.Ok(new CreateCarOutcome(CreateCarStatus.Created, created, null));
    }
}
