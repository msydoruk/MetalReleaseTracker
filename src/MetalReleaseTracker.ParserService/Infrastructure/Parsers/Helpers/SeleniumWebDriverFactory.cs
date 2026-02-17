using MetalReleaseTracker.ParserService.Infrastructure.Http.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;

public class SeleniumWebDriverFactory : ISeleniumWebDriverFactory
{
    private const string StealthScript = """
        Object.defineProperty(navigator, 'webdriver', { get: () => undefined });

        window.navigator.chrome = { runtime: {}, };

        Object.defineProperty(navigator, 'plugins', {
            get: () => [1, 2, 3, 4, 5],
        });

        Object.defineProperty(navigator, 'languages', {
            get: () => ['en-US', 'en'],
        });

        const originalQuery = window.navigator.permissions.query;
        window.navigator.permissions.query = (parameters) =>
            parameters.name === 'notifications'
                ? Promise.resolve({ state: Notification.permission })
                : originalQuery(parameters);
        """;

    private readonly IUserAgentProvider _userAgentProvider;
    private readonly ILogger<SeleniumWebDriverFactory> _logger;

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
        options.AddAdditionalOption("useAutomationExtension", false);
        options.AddArgument($"--user-agent={_userAgentProvider.GetRandomUserAgent()}");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--lang=en-US,en");

        _logger.LogInformation("Creating ChromeDriver (Selenium Manager will resolve chromedriver automatically).");
        var driver = new ChromeDriver(options);

        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(30);

        InjectStealthScripts(driver);

        return driver;
    }

    private void InjectStealthScripts(ChromeDriver driver)
    {
        try
        {
            var parameters = new Dictionary<string, object> { ["source"] = StealthScript };
            driver.ExecuteCdpCommand("Page.addScriptToEvaluateOnNewDocument", parameters);
            _logger.LogInformation("Injected stealth scripts via CDP.");
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to inject stealth scripts via CDP.");
        }
    }
}
