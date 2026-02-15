using System.Diagnostics;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;

public class SeleniumWebDriverFactory : ISeleniumWebDriverFactory
{
    private readonly IUserAgentProvider _userAgentProvider;
    private readonly ILogger<SeleniumWebDriverFactory> _logger;

    private static readonly string[] ChromeDriverSearchPaths =
    [
        "/usr/bin/chromedriver",
        "/usr/lib/chromium/chromedriver",
        "/usr/lib/chromium-browser/chromedriver"
    ];

    public SeleniumWebDriverFactory(IUserAgentProvider userAgentProvider, ILogger<SeleniumWebDriverFactory> logger)
    {
        _userAgentProvider = userAgentProvider;
        _logger = logger;
    }

    public IWebDriver CreateDriver()
    {
        var options = new ChromeOptions();

        var chromeBin = Environment.GetEnvironmentVariable("CHROME_BIN");
        if (!string.IsNullOrEmpty(chromeBin))
        {
            options.BinaryLocation = chromeBin;
            _logger.LogInformation("Using Chrome binary: {ChromeBin}", chromeBin);
        }

        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddExcludedArgument("enable-automation");
        options.AddArgument($"--user-agent={_userAgentProvider.GetRandomUserAgent()}");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--remote-debugging-pipe");

        var driverPath = FindChromeDriver();

        if (driverPath != null)
        {
            _logger.LogInformation("Using ChromeDriver at: {DriverPath}", driverPath);
            LogChromeDriverVersion(driverPath);

            var service = ChromeDriverService.CreateDefaultService(
                Path.GetDirectoryName(driverPath)!,
                Path.GetFileName(driverPath));
            service.WhitelistedIPAddresses = "";
            service.EnableVerboseLogging = true;
            service.LogPath = "/tmp/chromedriver.log";

            return CreateDriver(service, options);
        }

        _logger.LogInformation("ChromeDriver not found at known paths, using Selenium auto-detection.");
        return CreateDriver(null, options);
    }

    private ChromeDriver CreateDriver(ChromeDriverService? service, ChromeOptions options)
    {
        var driver = service != null
            ? new ChromeDriver(service, options)
            : new ChromeDriver(options);

        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

        return driver;
    }

    private void LogChromeDriverVersion(string driverPath)
    {
        try
        {
            var psi = new ProcessStartInfo(driverPath, "--version")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using var process = Process.Start(psi);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(5000);
                _logger.LogInformation("ChromeDriver version: {Version}", output.Trim());
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get ChromeDriver version from {DriverPath}", driverPath);
        }
    }

    private string? FindChromeDriver()
    {
        var envPath = Environment.GetEnvironmentVariable("CHROMEDRIVER_PATH");
        if (!string.IsNullOrEmpty(envPath) && File.Exists(envPath))
        {
            return envPath;
        }

        foreach (var path in ChromeDriverSearchPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
}
