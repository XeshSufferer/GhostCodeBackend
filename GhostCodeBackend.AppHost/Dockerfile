FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GhostCodeBackend.AppHost/GhostCodeBackend.AppHost.csproj", "GhostCodeBackend.AppHost/"]
COPY ["GhostCodeBackend.AccountsManagementService/GhostCodeBackend.AccountsManagementService.csproj", "GhostCodeBackend.AccountsManagementService/"]
COPY ["GhostCodeBackend.ServiceDefaults/GhostCodeBackend.ServiceDefaults.csproj", "GhostCodeBackend.ServiceDefaults/"]
RUN dotnet restore "GhostCodeBackend.AppHost/GhostCodeBackend.AppHost.csproj"
COPY . .
WORKDIR "/src/GhostCodeBackend.AppHost"
RUN dotnet build "./GhostCodeBackend.AppHost.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GhostCodeBackend.AppHost.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GhostCodeBackend.AppHost.dll"]
