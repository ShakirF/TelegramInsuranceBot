# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . ./
RUN dotnet restore
RUN dotnet publish TelegramBot/TelegramBot.csproj -c Release -o /app/out

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out ./

COPY TelegramBot/appsettings.Production.json ./appsettings.Production.json
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "TelegramBot.dll"]
