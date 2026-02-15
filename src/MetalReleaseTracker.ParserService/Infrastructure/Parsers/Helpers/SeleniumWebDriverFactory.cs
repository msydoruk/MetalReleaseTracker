using MetalReleaseTracker.ParserService.Infrastructure.Http.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;

public class SeleniumWebDriverFactory : ISeleniumWebDriverFactory
{
    private readonly IUserAgentProvider _userAgentProvider;

    public SeleniumWebDriverFactory(IUserAgentProvider userAgentProvider)
    {
        _userAgentProvider = userAgentProvider;
    }

    public IWebDriver CreateDriver()
    {
        var options = new ChromeOptions();

        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddExcludedArgument("enable-automation");
        options.AddArgument($"--user-agent={_userAgentProvider.GetRandomUserAgent()}");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--window-size=1920,1080");

        var driver = new ChromeDriver(options);

        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

        return driver;
    }
}
