# Frontend build stage (skipped when SKIP_FRONTEND_BUILD=true and pre-built build/ exists)
FROM node:18-alpine AS frontend-build
WORKDIR /app

ARG SKIP_FRONTEND_BUILD=false

COPY MetalReleaseTracker.Frontend/package.json MetalReleaseTracker.Frontend/package-lock.json* ./

RUN if [ "$SKIP_FRONTEND_BUILD" = "false" ]; then \
      npm install --legacy-peer-deps; \
    fi

COPY MetalReleaseTracker.Frontend/ .

ARG REACT_APP_API_URL=/api
ENV REACT_APP_API_URL=$REACT_APP_API_URL

ENV NODE_OPTIONS=--max-old-space-size=2048
ENV GENERATE_SOURCEMAP=false

RUN if [ "$SKIP_FRONTEND_BUILD" = "false" ]; then \
      npm run build; \
    fi

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
