FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MetalReleaseTracker.API/MetalReleaseTracker.API.csproj", "MetalReleaseTracker.API/"]
COPY ["MetalReleaseTracker.Infrastructure/MetalReleaseTracker.Infrastructure.csproj", "MetalReleaseTracker.Infrastructure/"]
RUN dotnet restore "./MetalReleaseTracker.API/MetalReleaseTracker.API.csproj"

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

COPY . .
WORKDIR "/src/MetalReleaseTracker.API"
RUN dotnet build "./MetalReleaseTracker.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./MetalReleaseTracker.API.csproj" -c Release -o /app/publish

FROM build AS final
WORKDIR /app
COPY --from=publish /app/publish . 

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "MetalReleaseTracker.API.dll"]