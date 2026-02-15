using OpenQA.Selenium;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;

public interface ISeleniumWebDriverFactory
{
    IWebDriver CreateDriver();
}
