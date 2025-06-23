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

        /// <summary>
        /// Finds element with smart wait - waits for element to be displayed and enabled
        /// </summary>
        protected IWebElement FindElementWithWait(By locator, int timeoutSeconds = 15)
        {
            WaitForLoaderToDisappear();
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(driver => {
                try
                {
                    var element = driver.FindElement(locator);
                    return element.Displayed && element.Enabled ? element : null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            });
        }

        /// <summary>
        /// Finds multiple elements with wait - returns when at least one element is found
        /// </summary>
        protected IList<IWebElement> FindElementsWithWait(By locator, int timeoutSeconds = 15)
        {
            WaitForLoaderToDisappear();
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(driver => {
                var elements = driver.FindElements(locator);
                return elements.Count > 0 ? elements : null;
            });
        }

        /// <summary>
        /// Finds elements without explicit wait - only waits for loader to disappear
        /// </summary>
        protected IList<IWebElement> FindElementsWithoutWait(By locator)
        {
            WaitForLoaderToDisappear();
            return Driver.FindElements(locator);
        }

        /// <summary>
        /// Waits for a single element to disappear from the page
        /// </summary>
        /// <param name="locator">Element locator</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>True if element disappeared, false if timeout</returns>
        public bool WaitForElementToDisappear(By locator, int timeoutSeconds = 30)
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(driver => {
                    var elements = driver.FindElements(locator);
                    return elements.Count == 0 || !elements[0].Displayed;
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Waits for multiple elements to disappear from the page
        /// </summary>
        /// <param name="locator">Elements locator</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>True if all elements disappeared, false if timeout</returns>
        public bool WaitForElementsToDisappear(By locator, int timeoutSeconds = 30)
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(driver => {
                    var elements = driver.FindElements(locator);
                    return elements.Count == 0 || elements.All(element => !element.Displayed);
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Waits for loading indicators to disappear from the page
        /// </summary>
        protected void WaitForLoaderToDisappear(int timeoutSeconds = 30)
        {
            try
            {
                bool disappeared = WaitForElementsToDisappear(By.CssSelector("div.loader"), timeoutSeconds);
                if (disappeared)
                {
                    LogHelper.LogInfo("Loader disappeared", ExtentTest);
                }
                else
                {
                    LogHelper.LogInfo("Loader wait timeout", ExtentTest);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"Loader wait error: {ex.Message}", ExtentTest);
            }
        }

        /// <summary>
        /// Forces thread to wait for specified seconds - use sparingly
        /// </summary>
        protected void ForcedWait(int seconds)
        {
            LogHelper.LogInfo($"Forced wait for {seconds} seconds", ExtentTest);
            System.Threading.Thread.Sleep(seconds * 1000);
        }

        /// <summary>
        /// Waits for page to be fully loaded including JavaScript and jQuery
        /// </summary>
        protected void WaitForPageFullyLoaded(int timeoutSeconds = 30)
        {
            try
            {
                LogHelper.LogInfo("Waiting for page to fully load", ExtentTest);
                
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(driver => {
                    // Page loading complete
                    var pageReady = ((IJavaScriptExecutor)driver)
                        .ExecuteScript("return document.readyState").Equals("complete");
                    
                    // jQuery loading complete (if exists)
                    var jqueryReady = (bool)((IJavaScriptExecutor)driver)
                        .ExecuteScript("return typeof jQuery == 'undefined' || jQuery.active == 0");
                        
                    return pageReady && jqueryReady;
                });
                
                LogHelper.LogInfo("Page fully loaded", ExtentTest);
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"Page load wait timeout: {ex.Message}", ExtentTest);
            }
        }

        /// <summary>
        /// Checks if error message with specified text is displayed on page
        /// </summary>
        public bool IsErrorMessageDisplayed(string errorText)
        {
            try
            {
                By errorLocator = By.XPath($"//*[contains(text(),'{errorText}')]");
                var elements = Driver.FindElements(errorLocator);
                bool isDisplayed = elements.Count > 0 && elements[0].Displayed;

                if (isDisplayed)
                {
                    LogHelper.LogInfo($"Found error message: '{errorText}'", ExtentTest);
                }
                else
                {
                    LogHelper.LogInfo($"Error message not found: '{errorText}'", ExtentTest);
                }

                return isDisplayed;
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error checking error message: {ex.Message}", ex, ExtentTest);
                return false;
            }
        }

        /// <summary>
        /// Navigates to specified URL and returns new page instance
        /// </summary>
        public TPage NavigateTo<TPage>(string url) where TPage : BasePage
        {
            try
            {
                Driver.Navigate().GoToUrl(url);
                LogHelper.LogInfo($"Navigated to {url}", ExtentTest);

                var page = (TPage)Activator.CreateInstance(typeof(TPage), Driver, ExtentTest);
                return page;
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Navigation to {url} failed", ex, ExtentTest);
                throw;
            }
        }

    }
}
