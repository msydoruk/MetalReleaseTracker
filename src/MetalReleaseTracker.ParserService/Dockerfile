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

# Install Google Chrome stable (Debian snap packages for chromium don't work in Docker)
RUN apt-get update && apt-get install -y --no-install-recommends \
    wget gnupg2 \
    fonts-liberation libnss3 libxss1 libasound2t64 \
    libatk-bridge2.0-0 libgtk-3-0 xdg-utils \
    && wget -q -O /tmp/google-chrome.deb https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb \
    && apt-get install -y --no-install-recommends /tmp/google-chrome.deb \
    && rm /tmp/google-chrome.deb \
    && rm -rf /var/lib/apt/lists/*

# Verify Chrome is installed
RUN google-chrome --version

ENV CHROME_BIN=/usr/bin/google-chrome

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MetalReleaseTracker.ParserService.dll"]
