# syntax=docker/dockerfile:1
#
# Build context is the repository root, not ./telegram-bot — this image needs
# the shared Doniyors.Data project as well. See docker-compose.yml.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Restore first, on its own layer, so a source-only change does not re-restore.
COPY shared/Doniyors.Data/Doniyors.Data.csproj shared/Doniyors.Data/
COPY telegram-bot/telegram-bot.csproj telegram-bot/
RUN dotnet restore telegram-bot/telegram-bot.csproj

COPY shared/ shared/
COPY telegram-bot/ telegram-bot/

RUN dotnet publish telegram-bot/telegram-bot.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

# Kestrel binds this port; the image's non-root "app" user may not use <1024.
ENV ASPNETCORE_HTTP_PORTS=8081

EXPOSE 8081

USER $APP_UID

ENTRYPOINT ["dotnet", "telegram-bot.dll"]
