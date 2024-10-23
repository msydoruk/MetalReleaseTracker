FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "./MetalReleaseTracker.BackgroundServices/MetalReleaseTracker.BackgroundServices.csproj"

WORKDIR "/src/MetalReleaseTracker.BackgroundServices"
RUN dotnet publish "./MetalReleaseTracker.BackgroundServices.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MetalReleaseTracker.BackgroundServices.dll"]