// Pages/HomePage.cs
using AventStack.ExtentReports;
using OpenQA.Selenium;
using System;
using PayoneerUIAssignment.Common;
using PayoneerUIAssignment.Pages;
using PayoneerUIAssignment.Pages;

namespace PayoneerUIAssignment.Pages
{
    public class HomePage : BasePage
    {
        private readonly By userGreeting = By.CssSelector("span.hi-user.containMiniTitle.ng-binding");
        private readonly By menuSearch = By.Id("menuSearch");
        private readonly By searchInput = By.Id("autoComplete");


        public HomePage(IWebDriver driver, ExtentTest extentTest = null) : base(driver, extentTest)
        {
        }

        public bool IsLoggedIn(string expectedUsername = null)
        {
            try
            {
                var userElement = FindElementWithWait(userGreeting);
                
                if (!string.IsNullOrEmpty(expectedUsername))
                {
                    string actualUsername = userElement.Text;
                    LogHelper.LogInfo($"期望用户名: {expectedUsername}, 实际用户名: {actualUsername}", ExtentTest);
                    return actualUsername.Equals(expectedUsername, StringComparison.OrdinalIgnoreCase);
                }
                
                LogHelper.LogInfo($"用户问候元素显示状态: {userElement.Displayed}", ExtentTest);
                return userElement.Displayed;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"检查登录状态失败: {ex.Message}", ExtentTest);
                return false;
            }
        }

        public string GetLoggedInUsername()
        {
            try
            {
                var userElement = FindElementWithWait(userGreeting);
                string username = userElement.Text;
                LogHelper.LogInfo($"获取到登录用户名: {username}", ExtentTest);
                return username;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"获取登录用户名失败: {ex.Message}", ExtentTest);
                return null;
            }
        }

        public ShoppingCartPage NavigateToShoppingCart()
        {
            try
            {
                LogHelper.LogInfo("从主页导航到购物车", ExtentTest);
                var cartPage = new ShoppingCartPage(Driver, ExtentTest);
                return cartPage.NavigateToCart();
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"导航到购物车失败: {ex.Message}", ExtentTest);
                throw;
            }
        }

        public SearchCategories SearchProduct(string productName)
        {
            try
            {
                LogHelper.LogInfo($"开始搜索商品: {productName}", ExtentTest);
                
                FindElementWithWait(menuSearch).Click();
                var searchInputElement = FindElementWithWait(searchInput);
                searchInputElement.Clear();
                searchInputElement.SendKeys(productName);
                searchInputElement.SendKeys(Keys.Enter);
                ForcedWait(5);
                
                LogHelper.LogInfo($"已提交搜索: {productName}", ExtentTest);
                return new SearchCategories(Driver, ExtentTest);
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"搜索商品失败: {ex.Message}", ExtentTest);
                throw;
            }
        }

        public class SearchCategories : BasePage
        {
            private readonly By productElement = By.CssSelector("li[ng-repeat*='product in']");
            private readonly By productNameText = By.CssSelector("li[ng-repeat*='product in'] a.productName.ng-binding");

            public SearchCategories(IWebDriver driver, ExtentTest extentTest = null) : base(driver, extentTest)
            {
            }

            public bool HasSearchResults(string expectedProductName = null)
            {
                try
                {
                    LogHelper.LogInfo("检查搜索结果", ExtentTest);
                    
                    // 检查商品元素是否存在
                    try
                    {
                        var productElements = FindElementsWithWait(productElement, 5);
                        if (productElements == null || productElements.Count == 0)
                        {
                            LogHelper.LogInfo("未找到商品搜索结果", ExtentTest);
                            return false;
                        }
                    }
                    catch
                    {
                        LogHelper.LogInfo("未找到商品搜索结果", ExtentTest);
                        return false;
                    }
                    
                    // 获取商品名称文本
                    var nameElement = FindElementWithWait(productNameText);
                    string actualProductText = nameElement.Text;
                    
                    if (!string.IsNullOrEmpty(expectedProductName))
                    {
                        bool exactMatch = actualProductText.Equals(expectedProductName, StringComparison.OrdinalIgnoreCase);
                        LogHelper.LogInfo($"期望商品: {expectedProductName}, 实际结果: {actualProductText}, 精确匹配: {exactMatch}", ExtentTest);
                        return exactMatch;
                    }
                    
                    LogHelper.LogInfo($"搜索结果: {actualProductText}", ExtentTest);
                    return !string.IsNullOrEmpty(actualProductText);
                }
                catch (Exception ex)
                {
                    LogHelper.LogInfo($"检查搜索结果失败: {ex.Message}", ExtentTest);
                    return false;
                }
            }

            public ProductPage SelectFirstProduct()
            {
                try
                {
                    LogHelper.LogInfo("点击进入第一个搜索结果商品", ExtentTest);
                    
                    var firstProduct = FindElementWithWait(productElement);
                    firstProduct.Click();
                    WaitForPageFullyLoaded();
                    
                    LogHelper.LogInfo("已点击商品，跳转到商品详情页", ExtentTest);
                    return new ProductPage(Driver, ExtentTest);
                }
                catch (Exception ex)
                {
                    LogHelper.LogInfo($"点击商品失败: {ex.Message}", ExtentTest);
                    throw;
                }
            }


        }
    }
}
