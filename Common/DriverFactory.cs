using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using System;
using System.IO;


namespace PayoneerUIAssignment.Common
{
    public class DriverFactory
    {
        /// <summary>
        /// Creates a WebDriver instance for the specified browser
        /// </summary>
        /// <param name="browser">Browser name (only Chrome is supported)</param>
        /// <returns>IWebDriver instance</returns>
        public static IWebDriver CreateDriver(string browser)
        {
            if (string.IsNullOrWhiteSpace(browser))
            {
                throw new ArgumentException("Browser name cannot be null or empty", nameof(browser));
            }

            IWebDriver driver = null;
            bool headless = Environment.GetEnvironmentVariable("HeadlessMode")?.ToLower() == "true";
            Logger.Info($"Creating {browser} driver, Headless mode: {headless}");

            if (browser.ToLower() != "chrome")
            {
                throw new ArgumentException($"Unsupported browser: {browser}. Only Chrome is supported");
            }

            driver = CreateChromeDriverWithTimeout(headless);

            return driver;
        }

        /// <summary>
        /// Creates Chrome driver with error handling and logging
        /// </summary>
        /// <param name="headless">Whether to run Chrome in headless mode</param>
        /// <returns>Chrome WebDriver instance</returns>
        private static IWebDriver CreateChromeDriverWithTimeout(bool headless)
        {
            try
            {
                Logger.Info("Creating Chrome driver");
                return CreateChromeDriver(headless);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create Chrome driver: {ex.Message}", ex);
                throw new InvalidOperationException($"Chrome driver creation failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Configures and creates Chrome driver with specified options
        /// </summary>
        /// <param name="headless">Whether to run Chrome in headless mode</param>
        /// <returns>Configured Chrome WebDriver instance</returns>
        private static IWebDriver CreateChromeDriver(bool headless)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--start-maximized");
            chromeOptions.AddArgument("--ignore-certificate-errors");
            chromeOptions.PageLoadTimeout = TimeSpan.FromSeconds(30);

            if (headless)
            {
                chromeOptions.AddArgument("--headless=new");
                chromeOptions.AddArgument("--disable-gpu");
                chromeOptions.AddArgument("--no-sandbox");
                chromeOptions.AddArgument("--disable-dev-shm-usage");
            }

            // Use fixed driver path
            string driverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "drivers");
            Logger.Info($"Using driver path: {driverPath}");

            if (!Directory.Exists(driverPath))
            {
                Logger.Info($"Driver path does not exist: {driverPath}");
                // If driver path doesn't exist, try using system Chrome
                Logger.Info("Attempting to create ChromeDriver using system Chrome");
                return new ChromeDriver(chromeOptions);
            }

            return new ChromeDriver(driverPath, chromeOptions);
        }
    }
}
