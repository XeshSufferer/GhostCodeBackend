# ------- runtime -------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ------- build -------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Копируем оба .csproj (относительно КОРНЯ солюшена – на один уровень выше)
COPY ["GhostCodeBackend.AccountsManagementService/GhostCodeBackend.AccountsManagementService.csproj", "GhostCodeBackend.AccountsManagementService/"]
COPY ["GhostCodeBackend.ServiceDefaults/GhostCodeBackend.ServiceDefaults.csproj", "GhostCodeBackend.ServiceDefaults/"]

# 2. Восстанавливаем зависимости (кэшируем слой)
RUN dotnet restore "GhostCodeBackend.AccountsManagementService/GhostCodeBackend.AccountsManagementService.csproj"

# 3. Копируем весь исходный код (опять же относительно корня солюшена)
COPY . .

# 4. Собираем проект
RUN dotnet build "GhostCodeBackend.AccountsManagementService/GhostCodeBackend.AccountsManagementService.csproj" \
               -c $BUILD_CONFIGURATION -o /app/build

# ------- publish -------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GhostCodeBackend.AccountsManagementService/GhostCodeBackend.AccountsManagementService.csproj" \
                  -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ------- final -------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GhostCodeBackend.AccountsManagementService.dll"]