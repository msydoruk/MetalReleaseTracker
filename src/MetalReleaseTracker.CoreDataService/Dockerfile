# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["MetalReleaseTracker.CoreDataService/MetalReleaseTracker.CoreDataService.csproj", "MetalReleaseTracker.CoreDataService/"]
COPY ["MetalReleaseTracker.SharedLibraries/MetalReleaseTracker.SharedLibraries.csproj", "MetalReleaseTracker.SharedLibraries/"]
RUN dotnet restore "MetalReleaseTracker.CoreDataService/MetalReleaseTracker.CoreDataService.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/MetalReleaseTracker.CoreDataService"
RUN dotnet build "MetalReleaseTracker.CoreDataService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MetalReleaseTracker.CoreDataService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MetalReleaseTracker.CoreDataService.dll"]
