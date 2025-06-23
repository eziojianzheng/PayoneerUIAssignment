using AventStack.ExtentReports;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using OpenQA.Selenium;
using System;
using System.IO;

namespace PayoneerUIAssignment.Common
{
    [TestClass]
    public class TestBase
    {
        protected IWebDriver Driver;
        protected IConfigurationRoot Config;
        protected ExtentTest ExtentTest;

        public TestContext TestContext { get; set; }

        /// <summary>
        /// Initializes test class setup - creates required directories
        /// </summary>
        [ClassInitialize]
        public static void ClassSetup(TestContext testContext)
        {
            // Ensure Screenshots directory exists
            var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
            if (!Directory.Exists(screenshotDir))
            {
                Directory.CreateDirectory(screenshotDir);
            }
        }

        /// <summary>
        /// Cleans up test class resources and generates final report
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Generate final report
            ExtentReportManager.Flush();
        }

        /// <summary>
        /// Sets up individual test - initializes driver, config, and reporting
        /// </summary>
        [TestInitialize]
        public void TestSetup()
        {
            // Create test report entry
            ExtentTest = ExtentReportManager.CreateTest(TestContext.TestName);

            LogHelper.LogInfo($"Starting test execution: {TestContext.TestName}", ExtentTest);

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Config/appsettings.json")
                .AddEnvironmentVariables(); // Add environment variable support

            Config = builder.Build();

            // Priority: test parameters > environment variables > config file
            string browser = TestContext.Properties["Browser"]?.ToString()
                ?? Environment.GetEnvironmentVariable("Browser")
                ?? Config["Browser"];

            LogHelper.LogInfo($"Using browser: {browser}", ExtentTest);

            Driver = DriverFactory.CreateDriver(browser);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            LogHelper.LogInfo("Browser driver initialized successfully", ExtentTest);
        }

        /// <summary>
        /// Cleans up individual test - handles screenshots, logging, and driver cleanup
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                // Take screenshot on test failure
                if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
                {
                    LogHelper.LogFail($"Test failed: {TestContext.CurrentTestOutcome}", ExtentTest);

                    // Capture screenshot and add to report
                    string screenshotPath = TakeScreenshot("failure-screenshot");
                    if (!string.IsNullOrEmpty(screenshotPath))
                    {
                        ExtentTest.AddScreenCaptureFromPath(screenshotPath);
                    }
                }
                else
                {
                    LogHelper.LogPass("Test passed successfully", ExtentTest);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error during test cleanup: {ex.Message}", ex, ExtentTest);
            }
            finally
            {
                if (Driver != null)
                {
                    LogHelper.LogInfo("Closing browser driver", ExtentTest);
                    Driver.Quit();
                    Driver = null;
                }
                try
                {
                    ExtentReportManager.Flush();
                }
                catch (Exception flushEx)
                {
                    // Prevent flush exceptions from affecting test cleanup
                    Console.WriteLine($"ExtentReport flush error: {flushEx.Message}");
                }
            }
        }

        /// <summary>
        /// Captures screenshot with timestamp and saves to Screenshots directory
        /// </summary>
        /// <param name="name">Screenshot filename prefix</param>
        /// <returns>Full path to saved screenshot file</returns>
        protected string TakeScreenshot(string name = "screenshot")
        {
            try
            {
                if (Driver is ITakesScreenshot ts)
                {
                    LogHelper.LogInfo($"Taking screenshot: {name}", ExtentTest);
                    var screenshot = ts.GetScreenshot();

                    // Save screenshot to filesystem for debugging
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
                    if (!Directory.Exists(screenshotDir))
                    {
                        Directory.CreateDirectory(screenshotDir);
                    }

                    var screenshotPath = Path.Combine(screenshotDir, $"{name}_{timestamp}.png");
                    screenshot.SaveAsFile(screenshotPath);
                    LogHelper.LogInfo($"Screenshot saved to: {screenshotPath}", ExtentTest);
                    return screenshotPath;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Screenshot capture failed: {ex.Message}", ex, ExtentTest);
            }
            return null;
        }
    }
}
