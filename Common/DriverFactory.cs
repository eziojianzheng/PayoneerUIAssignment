using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PayoneerUIAssignment.Common
{
    public class DriverFactory
    {
        // 设置驱动程序启动超时时间（秒）
        private const int DriverStartTimeoutSeconds = 30;

        public static IWebDriver CreateDriver(string browser)
        {
            IWebDriver driver = null;
            bool headless = Environment.GetEnvironmentVariable("HeadlessMode")?.ToLower() == "true";

            switch (browser?.ToLower())
            {
                case "chrome":
                    driver = CreateChromeDriverWithTimeout(headless);
                    break;

                case "firefox":
                    var firefoxOptions = new FirefoxOptions();
                    firefoxOptions.AddArgument("--start-maximized");
                    if (headless)
                    {
                        firefoxOptions.AddArgument("--headless");
                    }
                    driver = new FirefoxDriver(firefoxOptions);
                    break;

                default:
                    throw new ArgumentException($"Unsupported browser: {browser}");
            }

            return driver;
        }

        private static IWebDriver CreateChromeDriverWithTimeout(bool headless)
        {
            IWebDriver driver = null;
            Exception lastException = null;

            // 创建一个取消令牌，用于超时控制
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DriverStartTimeoutSeconds)))
            {
                try
                {
                    // 在任务中启动 ChromeDriver
                    var driverTask = Task.Run(() =>
                    {
                        try
                        {
                            return CreateChromeDriver(headless);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"创建 ChromeDriver 失败: {ex.Message}", ex);
                            throw;
                        }
                    }, cts.Token);

                    // 等待任务完成或超时
                    if (driverTask.Wait(TimeSpan.FromSeconds(DriverStartTimeoutSeconds)))
                    {
                        driver = driverTask.Result;
                    }
                    else
                    {
                        throw new TimeoutException($"ChromeDriver 启动超时（{DriverStartTimeoutSeconds}秒）");
                    }
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException($"ChromeDriver 启动超时（{DriverStartTimeoutSeconds}秒）");
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Logger.Error($"创建 ChromeDriver 失败: {ex.Message}", ex);
                }
            }

            if (driver == null)
            {
                throw new WebDriverException("无法创建 ChromeDriver 实例", lastException);
            }

            return driver;
        }

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

            // 使用固定的驱动路径
            string driverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "drivers");
            Logger.Info($"使用驱动路径: {driverPath}");

            if (!Directory.Exists(driverPath))
            {
                Logger.Info($"驱动路径不存在: {driverPath}");
                // 如果驱动路径不存在，尝试使用系统 Chrome
                Logger.Info("尝试使用系统 Chrome 创建 ChromeDriver");
                return new ChromeDriver(chromeOptions);
            }

            return new ChromeDriver(driverPath, chromeOptions);
        }
    }
}
