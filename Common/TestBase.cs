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

            LogHelper.LogInfo($"开始执行测试: {TestContext.TestName}", ExtentTest);

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Config/appsettings.json")
                .AddEnvironmentVariables();// 添加环境变量支持

            Config = builder.Build();

            // 优先使用环境变量或测试参数
            string browser = TestContext.Properties["Browser"]?.ToString()
                ?? Environment.GetEnvironmentVariable("Browser")
                ?? Config["Browser"];

            LogHelper.LogInfo($"使用浏览器: {browser}", ExtentTest);

            Driver = DriverFactory.CreateDriver(browser);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            LogHelper.LogInfo("浏览器驱动初始化完成", ExtentTest);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                // 在测试失败时截图
                if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
                {
                    LogHelper.LogFail($"测试未通过: {TestContext.CurrentTestOutcome}", ExtentTest);

                    // 截图并添加到报告
                    string screenshotPath = TakeScreenshot("failure-screenshot");
                    if (!string.IsNullOrEmpty(screenshotPath))
                    {
                        ExtentTest.AddScreenCaptureFromPath(screenshotPath);
                    }
                }
                else
                {
                    LogHelper.LogPass("测试通过", ExtentTest);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"测试清理过程中出错: {ex.Message}", ex, ExtentTest);
            }
            finally
            {
                if (Driver != null)
                {
                    LogHelper.LogInfo("关闭浏览器驱动", ExtentTest);
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
                    LogHelper.LogInfo($"正在截图: {name}", ExtentTest);
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
                    LogHelper.LogInfo($"截图已保存到: {screenshotPath}", ExtentTest);
                    return screenshotPath;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"截图失败: {ex.Message}", ex, ExtentTest);
            }
            return null;
        }
    }
}
