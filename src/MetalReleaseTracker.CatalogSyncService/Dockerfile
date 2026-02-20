# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["MetalReleaseTracker.CatalogSyncService/MetalReleaseTracker.CatalogSyncService.csproj", "MetalReleaseTracker.CatalogSyncService/"]
COPY ["MetalReleaseTracker.SharedLibraries/MetalReleaseTracker.SharedLibraries.csproj", "MetalReleaseTracker.SharedLibraries/"]
RUN dotnet restore "MetalReleaseTracker.CatalogSyncService/MetalReleaseTracker.CatalogSyncService.csproj"

# Copy everything else and publish
COPY . .
WORKDIR "/src/MetalReleaseTracker.CatalogSyncService"
RUN dotnet publish "MetalReleaseTracker.CatalogSyncService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MetalReleaseTracker.CatalogSyncService.dll"]
