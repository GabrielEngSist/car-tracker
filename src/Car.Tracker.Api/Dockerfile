FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ./src ./src

WORKDIR /src/src/Car.Tracker.Api
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish /p:SkipClientBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Car.Tracker.Api.dll"]

