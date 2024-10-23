FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "./MetalReleaseTracker.API/MetalReleaseTracker.API.csproj"

WORKDIR "/src/MetalReleaseTracker.API"
RUN dotnet publish "./MetalReleaseTracker.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MetalReleaseTracker.API.dll"]