FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MetalReleaseTracker.BackgroundServices/MetalReleaseTracker.BackgroundServices.csproj", "MetalReleaseTracker.BackgroundServices/"]
COPY ["MetalReleaseTracker.Infrastructure/MetalReleaseTracker.Infrastructure.csproj", "MetalReleaseTracker.Infrastructure/"]
RUN dotnet restore "./MetalReleaseTracker.BackgroundServices/MetalReleaseTracker.BackgroundServices.csproj"

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

COPY . .
WORKDIR "/src/MetalReleaseTracker.BackgroundServices"
RUN dotnet build "./MetalReleaseTracker.BackgroundServices.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "./MetalReleaseTracker.BackgroundServices.csproj" -c Release -o /app/publish

FROM build AS final
WORKDIR /app
COPY --from=publish /app/publish . 

ENTRYPOINT ["dotnet", "MetalReleaseTracker.BackgroundServices.dll"]