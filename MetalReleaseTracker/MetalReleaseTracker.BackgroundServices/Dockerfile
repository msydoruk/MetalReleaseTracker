FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MetalReleaseTracker.BackgroundServices/MetalReleaseTracker.BackgroundServices.csproj", "MetalReleaseTracker.BackgroundServices/"]
COPY ["MetalReleaseTracker.Application/MetalReleaseTracker.Application.csproj", "MetalReleaseTracker.Application/"]
COPY ["MetalReleaseTracker.Core/MetalReleaseTracker.Core.csproj", "MetalReleaseTracker.Core/"]
COPY ["MetalReleaseTracker.API/MetalReleaseTracker.API.csproj", "MetalReleaseTracker.API/"]
COPY ["MetalReleaseTracker.Infrastructure/MetalReleaseTracker.Infrastructure.csproj", "MetalReleaseTracker.Infrastructure/"]
RUN dotnet restore "./MetalReleaseTracker.BackgroundServices/./MetalReleaseTracker.BackgroundServices.csproj"
COPY . .
WORKDIR "/src/MetalReleaseTracker.BackgroundServices"
RUN dotnet build "./MetalReleaseTracker.BackgroundServices.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MetalReleaseTracker.BackgroundServices.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MetalReleaseTracker.BackgroundServices.dll"]