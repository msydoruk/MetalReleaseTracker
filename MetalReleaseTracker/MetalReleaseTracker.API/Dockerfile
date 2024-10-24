EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
USER app

COPY ["MetalReleaseTracker.API/MetalReleaseTracker.API.csproj", "MetalReleaseTracker.API/"]
RUN dotnet restore "./MetalReleaseTracker.API/./MetalReleaseTracker.API.csproj"

COPY . .
WORKDIR "/src/MetalReleaseTracker.API"
RUN dotnet build "./MetalReleaseTracker.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./MetalReleaseTracker.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .                 

ENTRYPOINT ["dotnet", "MetalReleaseTracker.API.dll"]