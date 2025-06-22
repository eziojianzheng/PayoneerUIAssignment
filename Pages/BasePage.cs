using System;
using System.Linq;
using AventStack.ExtentReports;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using PayoneerUIAssignment.Common;

namespace PayoneerUIAssignment.Pages
{
    public class BasePage
    {
        protected IWebDriver Driver;
        protected ExtentTest ExtentTest;

        protected BasePage(IWebDriver driver, ExtentTest extentTest = null)
        {
            Driver = driver;
            ExtentTest = extentTest;
        }

        public void SetExtentTest(ExtentTest extentTest)
        {
            ExtentTest = extentTest;
        }

        //为selenium每一个元素添加智能等待
        protected IWebElement FindElementWithWait(By locator, int timeoutSeconds = 15)
        {
            WaitForLoaderToDisappear();
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(driver => {
                var element = driver.FindElement(locator);
                return element.Displayed && element.Enabled ? element : null;
            });
        }

        protected IList<IWebElement> FindElementsWithWait(By locator, int timeoutSeconds = 15)
        {
            WaitForLoaderToDisappear();
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(driver => {
                var elements = driver.FindElements(locator);
                return elements.Count > 0 ? elements : null;
            });
        }

        protected IList<IWebElement> FindElementsWithoutWait(By locator)
        {
            WaitForLoaderToDisappear();
            return Driver.FindElements(locator);
        }

        protected void WaitForLoaderToDisappear(int timeoutSeconds = 30)
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(driver => {
                    var loaders = driver.FindElements(By.CssSelector("div.loader"));
                    return loaders.Count == 0 || loaders.All(loader => !loader.Displayed);
                });
                Logger.Info("加载器已消失");
            }
            catch (Exception ex)
            {
                Logger.Info($"等待加载器消失超时: {ex.Message}");
            }
        }

        protected void ForcedWait(int seconds)
        {
            Logger.Info($"强制等待 {seconds} 秒");
            System.Threading.Thread.Sleep(seconds * 1000);
        }

        protected void WaitForPageFullyLoaded(int timeoutSeconds = 30)
        {
            try
            {
                Logger.Info("等待页面完全加载");
                
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(driver => {
                    // 页面加载完成
                    var pageReady = ((IJavaScriptExecutor)driver)
                        .ExecuteScript("return document.readyState").Equals("complete");
                    
                    // jQuery加载完成（如果存在）
                    var jqueryReady = (bool)((IJavaScriptExecutor)driver)
                        .ExecuteScript("return typeof jQuery == 'undefined' || jQuery.active == 0");
                        
                    return pageReady && jqueryReady;
                });
                
                Logger.Info("页面已完全加载");
            }
            catch (Exception ex)
            {
                Logger.Info($"等待页面完全加载超时: {ex.Message}");
            }
        }

        public bool IsErrorMessageDisplayed(string errorText)
        {
            try
            {
                By errorLocator = By.XPath($"//*[contains(text(),'{errorText}')]");
                var elements = Driver.FindElements(errorLocator);
                bool isDisplayed = elements.Count > 0 && elements[0].Displayed;

                if (isDisplayed)
                {
                    Logger.Info($"找到错误消息: '{errorText}'");
                    ExtentTest?.Info($"找到错误消息: '{errorText}'");
                }
                else
                {
                    Logger.Info($"未找到错误消息: '{errorText}'");
                    ExtentTest?.Info($"未找到错误消息: '{errorText}'");
                }

                return isDisplayed;
            }
            catch (Exception ex)
            {
                Logger.Error($"检查错误消息时出错: {ex.Message}", ex);
                ExtentTest?.Error($"检查错误消息时出错: {ex.Message}");
                return false;
            }
        }


        public TPage NavigateTo<TPage>(string url) where TPage : BasePage
        {
            try
            {
                Driver.Navigate().GoToUrl(url);
                Logger.Info($"导航到 {url}");
                ExtentTest?.Info($"导航到 {url}");

                var page = (TPage)Activator.CreateInstance(typeof(TPage), Driver, ExtentTest);
                return page;
            }
            catch (Exception ex)
            {
                Logger.Error($"导航到 {url} 失败", ex);
                ExtentTest?.Error($"导航到 {url} 失败: {ex.Message}");
                throw;
            }
        }

    }
}
