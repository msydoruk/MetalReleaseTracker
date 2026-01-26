# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["MetalReleaseTracker.ParserService/MetalReleaseTracker.ParserService.csproj", "MetalReleaseTracker.ParserService/"]
COPY ["MetalReleaseTracker.SharedLibraries/MetalReleaseTracker.SharedLibraries.csproj", "MetalReleaseTracker.SharedLibraries/"]
RUN dotnet restore "MetalReleaseTracker.ParserService/MetalReleaseTracker.ParserService.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/MetalReleaseTracker.ParserService"
RUN dotnet build "MetalReleaseTracker.ParserService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MetalReleaseTracker.ParserService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MetalReleaseTracker.ParserService.dll"]
