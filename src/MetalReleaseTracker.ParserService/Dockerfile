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

# Verify Chromium and ChromeDriver installed correctly
RUN chromium --version && chromedriver --version

ENV CHROME_BIN=/usr/bin/chromium
ENV CHROMEDRIVER_PATH=/usr/bin/chromedriver

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MetalReleaseTracker.ParserService.dll"]
