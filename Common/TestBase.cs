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

        [ClassInitialize]
        public static void ClassSetup(TestContext testContext)
        {
            // 确保 Screenshots 目录存在
            var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
            if (!Directory.Exists(screenshotDir))
            {
                Directory.CreateDirectory(screenshotDir);
            }
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // 生成报告
            ExtentReportManager.Flush();
        }

        [TestInitialize]
        public void TestSetup()
        {
            // 创建测试报告条目
            ExtentTest = ExtentReportManager.CreateTest(TestContext.TestName);

            Logger.Info($"开始执行测试: {TestContext.TestName}");
            ExtentTest.Info($"开始执行测试: {TestContext.TestName}");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Config/appsettings.json")
                .AddEnvironmentVariables();// 添加环境变量支持

            Config = builder.Build();

            // 优先使用环境变量或测试参数
            string browser = TestContext.Properties["Browser"]?.ToString()
                ?? Environment.GetEnvironmentVariable("Browser")
                ?? Config["Browser"];

            Logger.Info($"使用浏览器: {browser}");
            ExtentTest.Info($"使用浏览器: {browser}");

            Driver = DriverFactory.CreateDriver(browser);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            Logger.Info("浏览器驱动初始化完成");
            ExtentTest.Info("浏览器驱动初始化完成");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                // 在测试失败时截图
                if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
                {
                    Logger.Info($"测试未通过: {TestContext.TestName}, 结果: {TestContext.CurrentTestOutcome}");
                    ExtentTest.Fail($"测试未通过: {TestContext.CurrentTestOutcome}");

                    // 截图并添加到报告
                    string screenshotPath = TakeScreenshot("failure-screenshot");
                    if (!string.IsNullOrEmpty(screenshotPath))
                    {
                        ExtentTest.AddScreenCaptureFromPath(screenshotPath);
                    }
                }
                else
                {
                    Logger.Info($"测试通过: {TestContext.TestName}");
                    ExtentTest.Pass("测试通过");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"测试清理过程中出错: {ex.Message}", ex);
                ExtentTest.Error($"测试清理过程中出错: {ex.Message}");
            }
            finally
            {
                if (Driver != null)
                {
                    Logger.Info("关闭浏览器驱动");
                    ExtentTest.Info("关闭浏览器驱动");
                    Driver.Quit();
                    Driver = null;
                }
                ExtentReportManager.Flush();
            }
        }

        protected string TakeScreenshot(string name = "screenshot")
        {
            try
            {
                if (Driver is ITakesScreenshot ts)
                {
                    Logger.Info($"正在截图: {name}");
                    var screenshot = ts.GetScreenshot();

                    // 保存截图到文件系统，便于调试
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
                    if (!Directory.Exists(screenshotDir))
                    {
                        Directory.CreateDirectory(screenshotDir);
                    }

                    var screenshotPath = Path.Combine(screenshotDir, $"{name}_{timestamp}.png");
                    screenshot.SaveAsFile(screenshotPath);
                    Logger.Info($"截图已保存到: {screenshotPath}");
                    return screenshotPath;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"截图失败: {ex.Message}", ex);
                ExtentTest.Error($"截图失败: {ex.Message}");
            }
            return null;
        }
    }
}
