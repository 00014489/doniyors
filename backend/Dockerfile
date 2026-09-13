# syntax=docker/dockerfile:1
#
# Build context is the repository root, not ./backend — this image needs the
# shared Doniyors.Data project as well. See docker-compose.yml.
#
# Two targets:
#   final       the API
#   migrations  a one-shot runner that applies the EF Core migrations

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Restore first, on its own layer, so a source-only change does not re-restore.
COPY shared/Doniyors.Data/Doniyors.Data.csproj shared/Doniyors.Data/
COPY backend/backend.csproj backend/
RUN dotnet restore backend/backend.csproj

COPY shared/ shared/
COPY backend/ backend/

RUN dotnet publish backend/backend.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


# ---------------------------------------------------------------- migrations
# An EF Core migration bundle: a single executable that brings the database up
# to the latest migration and exits. It reads the connection string from
# ConnectionStrings__DefaultConnection, like the API.
FROM build AS migrations-build

RUN dotnet tool install --global dotnet-ef --version 10.0.10

ENV PATH="${PATH}:/root/.dotnet/tools"

RUN dotnet ef migrations bundle \
    --project backend/backend.csproj \
    --configuration Release \
    --output /app/efbundle


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS migrations

WORKDIR /app

COPY --from=migrations-build /app/efbundle .

USER $APP_UID

ENTRYPOINT ["./efbundle"]


# --------------------------------------------------------------------- final
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

# Kestrel binds this port; the image's non-root "app" user may not use <1024.
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "backend.dll"]
