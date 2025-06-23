using AventStack.ExtentReports;
using OpenQA.Selenium;
using System;
using PayoneerUIAssignment.Common;

namespace PayoneerUIAssignment.Pages
{
    /// <summary>
    /// Home page object containing main navigation and search functionality
    /// </summary>
    public class HomePage : BasePage
    {
        private readonly By userGreeting = By.CssSelector("span.hi-user.containMiniTitle.ng-binding");
        private readonly By menuSearch = By.Id("menuSearch");
        private readonly By searchInput = By.Id("autoComplete");

        public HomePage(IWebDriver driver, ExtentTest extentTest = null) : base(driver, extentTest)
        {
        }

        /// <summary>
        /// Checks if user is logged in, optionally validates specific username
        /// </summary>
        public bool IsLoggedIn(string expectedUsername = null)
        {
            try
            {
                var userElement = FindElementWithWait(userGreeting);
                
                if (!string.IsNullOrEmpty(expectedUsername))
                {
                    string actualUsername = userElement.Text;
                    LogHelper.LogInfo($"Expected username: {expectedUsername}, Actual username: {actualUsername}", ExtentTest);
                    return actualUsername.Equals(expectedUsername, StringComparison.OrdinalIgnoreCase);
                }
                
                LogHelper.LogInfo($"User greeting element display status: {userElement.Displayed}", ExtentTest);
                return userElement.Displayed;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"Login status check failed: {ex.Message}", ExtentTest);
                return false;
            }
        }

        /// <summary>
        /// Gets the currently logged in username
        /// </summary>
        public string GetLoggedInUsername()
        {
            try
            {
                var userElement = FindElementWithWait(userGreeting);
                string username = userElement.Text;
                LogHelper.LogInfo($"Retrieved logged in username: {username}", ExtentTest);
                return username;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"Failed to get logged in username: {ex.Message}", ExtentTest);
                return null;
            }
        }

        /// <summary>
        /// Navigates to shopping cart page
        /// </summary>
        public ShoppingCartPage NavigateToShoppingCart()
        {
            try
            {
                LogHelper.LogInfo("Navigating from home page to shopping cart", ExtentTest);
                var cartPage = new ShoppingCartPage(Driver, ExtentTest);
                return cartPage.NavigateToCart();
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Navigation to shopping cart failed: {ex.Message}", ex, ExtentTest);
                throw;
            }
        }

        /// <summary>
        /// Searches for a product and returns search results page
        /// </summary>
        public SearchCategories SearchProduct(string productName)
        {
            try
            {
                LogHelper.LogInfo($"Starting product search: {productName}", ExtentTest);
                
                FindElementWithWait(menuSearch).Click();
                var searchInputElement = FindElementWithWait(searchInput);
                searchInputElement.Clear();
                searchInputElement.SendKeys(productName);
                searchInputElement.SendKeys(Keys.Enter);
                ForcedWait(5);
                
                LogHelper.LogInfo($"Search submitted: {productName}", ExtentTest);
                return new SearchCategories(Driver, ExtentTest);
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Product search failed: {ex.Message}", ex, ExtentTest);
                throw;
            }
        }

        /// <summary>
        /// Search results page containing product listings
        /// </summary>
        public class SearchCategories : BasePage
        {
            private readonly By productElement = By.CssSelector("li[ng-repeat*='product in']");
            private readonly By productNameText = By.CssSelector("li[ng-repeat*='product in'] a.productName.ng-binding");

            public SearchCategories(IWebDriver driver, ExtentTest extentTest = null) : base(driver, extentTest)
            {
            }

            /// <summary>
            /// Checks if search results are present, optionally validates specific product name
            /// </summary>
            public bool HasSearchResults(string expectedProductName = null)
            {
                try
                {
                    LogHelper.LogInfo("Checking search results", ExtentTest);
                    
                    // Check if product elements exist
                    var productElements = FindElementsWithoutWait(productElement);
                    if (productElements == null || productElements.Count == 0)
                    {
                        LogHelper.LogInfo("No product search results found", ExtentTest);
                        return false;
                    }
                    
                    // Get product name text
                    var nameElement = FindElementWithWait(productNameText);
                    string actualProductText = nameElement.Text;
                    
                    if (!string.IsNullOrEmpty(expectedProductName))
                    {
                        bool exactMatch = actualProductText.Equals(expectedProductName, StringComparison.OrdinalIgnoreCase);
                        LogHelper.LogInfo($"Expected product: {expectedProductName}, Actual result: {actualProductText}, Exact match: {exactMatch}", ExtentTest);
                        return exactMatch;
                    }
                    
                    LogHelper.LogInfo($"Search results: {actualProductText}", ExtentTest);
                    return !string.IsNullOrEmpty(actualProductText);
                }
                catch (Exception ex)
                {
                    LogHelper.LogInfo($"Search results check failed: {ex.Message}", ExtentTest);
                    return false;
                }
            }

            /// <summary>
            /// Clicks on the first product in search results
            /// </summary>
            public ProductPage SelectFirstProduct()
            {
                try
                {
                    LogHelper.LogInfo("Clicking first search result product", ExtentTest);
                    
                    var firstProduct = FindElementWithWait(productElement);
                    firstProduct.Click();
                    WaitForPageFullyLoaded();
                    
                    LogHelper.LogInfo("Product clicked, navigating to product details page", ExtentTest);
                    return new ProductPage(Driver, ExtentTest);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError($"Product click failed: {ex.Message}", ex, ExtentTest);
                    throw;
                }
            }
        }
    }
}
