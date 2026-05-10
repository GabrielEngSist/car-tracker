# syntax=docker/dockerfile:1.7
# Two-stage build:
#   1. dotnet-build -> dotnet publish (SPA build is skipped here; it's done on the host
#                      via the `prepackage` hook in azure.yaml, which produces
#                      src/Car.Tracker.Presentation/dist/).
#   2. runtime      -> aspnet base image + published app + dist/ copied into wwwroot.

# ---------- Stage 1: .NET publish ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dotnet-build
WORKDIR /src
COPY src/ ./src/
WORKDIR /src/src/Car.Tracker.Api
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish /p:SkipClientBuild=true

# ---------- Stage 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true
COPY --from=dotnet-build /app/publish ./
# SPA artifacts come from the host-built dist/ (see prepackage hook in azure.yaml).
COPY src/Car.Tracker.Presentation/dist/ ./wwwroot/
EXPOSE 8080
ENTRYPOINT ["dotnet", "Car.Tracker.Api.dll"]
