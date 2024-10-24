FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
USER app

COPY ["MetalReleaseTracker.BackgroundServices/MetalReleaseTracker.BackgroundServices.csproj", "MetalReleaseTracker.BackgroundServices/"]
RUN dotnet restore "./MetalReleaseTracker.BackgroundServices/MetalReleaseTracker.BackgroundServices.csproj"

COPY . .
WORKDIR "/src/MetalReleaseTracker.BackgroundServices"
RUN dotnet build "./MetalReleaseTracker.BackgroundServices.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./MetalReleaseTracker.BackgroundServices.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MetalReleaseTracker.BackgroundServices.dll"]