# Frontend build stage
FROM node:18-alpine AS frontend-build
WORKDIR /app

COPY MetalReleaseTracker.Frontend/package.json MetalReleaseTracker.Frontend/package-lock.json* ./
RUN npm install --legacy-peer-deps

COPY MetalReleaseTracker.Frontend/ .

ARG REACT_APP_API_URL=/api
ENV REACT_APP_API_URL=$REACT_APP_API_URL

ENV NODE_OPTIONS=--max-old-space-size=1024
ENV GENERATE_SOURCEMAP=false
RUN npm run build

# .NET build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["MetalReleaseTracker.CoreDataService/MetalReleaseTracker.CoreDataService.csproj", "MetalReleaseTracker.CoreDataService/"]
COPY ["MetalReleaseTracker.SharedLibraries/MetalReleaseTracker.SharedLibraries.csproj", "MetalReleaseTracker.SharedLibraries/"]
RUN dotnet restore "MetalReleaseTracker.CoreDataService/MetalReleaseTracker.CoreDataService.csproj"

# Copy everything else and publish
COPY . .
WORKDIR "/src/MetalReleaseTracker.CoreDataService"
RUN dotnet publish "MetalReleaseTracker.CoreDataService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
COPY --from=frontend-build /app/build ./wwwroot

ENTRYPOINT ["dotnet", "MetalReleaseTracker.CoreDataService.dll"]
