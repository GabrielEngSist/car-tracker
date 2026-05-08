using System.Net.Http;
using Car.Tracker.Application.Cqrs.Commands.Cars;
using Car.Tracker.Contracts;
using Car.Tracker.Domain.Integration.Wire;
using Car.Tracker.Domain.Ports.Integration;
using Car.Tracker.Tests.TestDoubles;
using Moq;

namespace Car.Tracker.Tests.Features.Cars;

public sealed class CreateCarCommandHandlerTests
{
    [Fact]
    public async Task Manual_create_happy_path_creates_car_and_returns_created()
    {
        var db = new FakeTrackerPersistence();
        var consultarPlaca = new Mock<IConsultarPlacaPort>(MockBehavior.Strict);
        var consultarFipe = new Mock<IConsultarPrecoFipePort>(MockBehavior.Strict);

        var handler = new CreateCarCommandHandler(db, consultarPlaca.Object, consultarFipe.Object);
        var req = new CreateCarRequest(
            Model: "Corolla",
            Year: 2018,
            CurrentKm: 12345,
            Name: "Daily",
            Placa: "ABC1D23",
            AutoBuscarDados: false);

        var r = await handler.Handle(new CreateCarCommand(req), CancellationToken.None);

        Assert.True(r.IsSuccess);
        Assert.Equal(CreateCarStatus.Created, r.Value!.Status);
        Assert.NotNull(r.Value.Car);
        Assert.Single(db.Cars);
        Assert.Equal(1, db.SaveChangesCallCount);
        Assert.Equal("Corolla", r.Value.Car!.Model);
        Assert.Equal("ABC1D23", r.Value.Car.Placa);
    }

    [Fact]
    public async Task Manual_create_returns_bad_request_when_model_missing()
    {
        var handler = new CreateCarCommandHandler(
            new FakeTrackerPersistence(),
            Mock.Of<IConsultarPlacaPort>(),
            Mock.Of<IConsultarPrecoFipePort>());

        var req = new CreateCarRequest(
            Model: null,
            Year: 2018,
            CurrentKm: 0,
            Name: null,
            Placa: null,
            AutoBuscarDados: false);

        var r = await handler.Handle(new CreateCarCommand(req), CancellationToken.None);

        Assert.Equal(CreateCarStatus.BadRequest, r.Value!.Status);
    }

    [Fact]
    public async Task Manual_create_returns_bad_request_when_year_invalid()
    {
        var handler = new CreateCarCommandHandler(
            new FakeTrackerPersistence(),
            Mock.Of<IConsultarPlacaPort>(),
            Mock.Of<IConsultarPrecoFipePort>());

        var req = new CreateCarRequest(
            Model: "Corolla",
            Year: 1800,
            CurrentKm: 0,
            Name: null,
            Placa: null,
            AutoBuscarDados: false);

        var r = await handler.Handle(new CreateCarCommand(req), CancellationToken.None);

        Assert.Equal(CreateCarStatus.BadRequest, r.Value!.Status);
    }

    [Fact]
    public async Task Manual_create_returns_bad_request_when_plate_invalid()
    {
        var handler = new CreateCarCommandHandler(
            new FakeTrackerPersistence(),
            Mock.Of<IConsultarPlacaPort>(),
            Mock.Of<IConsultarPrecoFipePort>());

        var req = new CreateCarRequest(
            Model: "Corolla",
            Year: 2018,
            CurrentKm: 0,
            Name: null,
            Placa: "INVALID",
            AutoBuscarDados: false);

        var r = await handler.Handle(new CreateCarCommand(req), CancellationToken.None);

        Assert.Equal(CreateCarStatus.BadRequest, r.Value!.Status);
    }

    [Fact]
    public async Task Auto_registration_creates_car_when_providers_return_ok()
    {
        var db = new FakeTrackerPersistence();
        var consultarPlaca = new Mock<IConsultarPlacaPort>(MockBehavior.Strict);
        var consultarFipe = new Mock<IConsultarPrecoFipePort>(MockBehavior.Strict);

        consultarPlaca
            .Setup(x => x.ConsultarPorPlacaAsync("ABC1D23", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConsultarPlacaResponse
            {
                Status = "ok",
                Dados = new ConsultarPlacaDadosRoot
                {
                    InformacoesVeiculo = new ConsultarPlacaInformacoesVeiculo
                    {
                        DadosVeiculo = new ConsultarPlacaDadosVeiculo
                        {
                            Placa = "ABC1D23",
                            Modelo = "COROLLA",
                            AnoModelo = "2018",
                        }
                    }
                }
            });

        consultarFipe
            .Setup(x => x.ConsultarPorPlacaAsync("ABC1D23", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConsultarPrecoFipeResponse { Status = "ok" });

        var handler = new CreateCarCommandHandler(db, consultarPlaca.Object, consultarFipe.Object);
        var req = new CreateCarRequest(
            Model: null,
            Year: null,
            CurrentKm: 1000,
            Name: null,
            Placa: "ABC1D23",
            AutoBuscarDados: true);

        var r = await handler.Handle(new CreateCarCommand(req), CancellationToken.None);

        Assert.Equal(CreateCarStatus.Created, r.Value!.Status);
        Assert.Single(db.Cars);
        Assert.NotNull(db.Cars.Single().ConsultaPlaca);
        Assert.NotNull(db.Cars.Single().ConsultaPrecoFipe);
    }

    [Fact]
    public async Task Auto_registration_returns_bad_request_when_plate_missing()
    {
        var handler = new CreateCarCommandHandler(
            new FakeTrackerPersistence(),
            Mock.Of<IConsultarPlacaPort>(),
            Mock.Of<IConsultarPrecoFipePort>());

        var req = new CreateCarRequest(
            Model: null,
            Year: null,
            CurrentKm: 1000,
            Name: null,
            Placa: "",
            AutoBuscarDados: true);

        var r = await handler.Handle(new CreateCarCommand(req), CancellationToken.None);

        Assert.Equal(CreateCarStatus.BadRequest, r.Value!.Status);
    }

    [Fact]
    public async Task Auto_registration_returns_bad_gateway_when_http_fails()
    {
        var handler = new CreateCarCommandHandler(
            new FakeTrackerPersistence(),
            Mock.Of<IConsultarPlacaPort>(_ => _.ConsultarPorPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()) == Task.FromException<ConsultarPlacaResponse?>(new HttpRequestException("down"))),
            Mock.Of<IConsultarPrecoFipePort>());

        var req = new CreateCarRequest(
            Model: null,
            Year: null,
            CurrentKm: 1000,
            Name: null,
            Placa: "ABC1D23",
            AutoBuscarDados: true);

        var r = await handler.Handle(new CreateCarCommand(req), CancellationToken.None);

        Assert.Equal(CreateCarStatus.BadGateway, r.Value!.Status);
    }
}

