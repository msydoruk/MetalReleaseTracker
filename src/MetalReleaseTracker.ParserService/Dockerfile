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

# Install Chromium and ChromeDriver for Selenium-based parsing
RUN apt-get update && apt-get install -y --no-install-recommends \
    chromium \
    chromium-driver \
    fonts-liberation \
    libnss3 \
    libxss1 \
    libasound2t64 \
    libatk-bridge2.0-0 \
    libgtk-3-0 \
    && rm -rf /var/lib/apt/lists/*

# Find where Chromium and ChromeDriver binaries are installed
RUN echo "=== Chromium files ===" && dpkg -L chromium 2>/dev/null | head -30 || true && \
    echo "=== ChromeDriver files ===" && dpkg -L chromium-driver 2>/dev/null | head -30 || true && \
    echo "=== Chromium binaries ===" && find / -name "chromium*" -type f 2>/dev/null || true && \
    echo "=== ChromeDriver binaries ===" && find / -name "chromedriver*" -type f 2>/dev/null || true

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MetalReleaseTracker.ParserService.dll"]
