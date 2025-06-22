using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AventStack.ExtentReports;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using PayoneerUIAssignment.Pages;
using PayoneerUIAssignment.Common;

namespace PayoneerUIAssignment.Pages
{
    public class ProductPage : BasePage
    {

        private readonly By productTitle = By.CssSelector("a.select.ng-binding");
        
        public ProductPage(IWebDriver driver, ExtentTest extentTest = null) : base(driver, extentTest) { }

        public bool IsProductPageLoaded()
        {
            try
            {
                Logger.Info("检查商品详情页是否加载成功");
                
                var titleElement = FindElementWithWait(productTitle);
                bool isLoaded = titleElement.Displayed;
                
                Logger.Info($"商品详情页加载状态: {(isLoaded ? "成功" : "失败")}");
                return isLoaded;
            }
            catch (Exception ex)
            {
                Logger.Info($"检查商品详情页失败: {ex.Message}");
                return false;
            }
        }

        public void SelectColor(string colorName)
        {
            try
            {
                Logger.Info($"选择颜色: {colorName}");
                
                By colorLocator = By.CssSelector($"span.productColor[title='{colorName.ToUpper()}']");
                var colorElement = FindElementWithWait(colorLocator);
                colorElement.Click();
                
                Logger.Info($"已选择颜色: {colorName}");
            }
            catch (Exception ex)
            {
                Logger.Info($"选择颜色{colorName}失败: {ex.Message}");
                throw;
            }
        }

        public void SetQuantity(int quantity)
        {
            try
            {
                Logger.Info($"设置商品数量: {quantity}");
                
                By quantityInput = By.Name("quantity");
                var quantityElement = FindElementWithWait(quantityInput);
                
                // 使用Backspace清理输入框
                quantityElement.Click(); // 确保焦点在输入框上
                quantityElement.SendKeys(Keys.Control + "a"); // 全选
                quantityElement.SendKeys(Keys.Backspace);     // 删除
                quantityElement.SendKeys(quantity.ToString());
                
                Logger.Info($"已设置商品数量: {quantity}");
            }
            catch (Exception ex)
            {
                Logger.Info($"设置商品数量{quantity}失败: {ex.Message}");
                throw;
            }
        }

        public bool IsAddToCartSuccessful()
        {
            try
            {
                Logger.Info("检查商品是否成功添加到购物车");
                
                By checkoutPopup = By.Id("checkOutPopUp");
                var popupElement = FindElementWithWait(checkoutPopup);
                bool isSuccessful = popupElement.Displayed;
                
                Logger.Info($"添加到购物车状态: {(isSuccessful ? "成功" : "失败")}");
                return isSuccessful;
            }
            catch (Exception ex)
            {
                Logger.Info($"检查添加购物车状态失败: {ex.Message}");
                return false;
            }
        }

        public void WaitForCheckoutPopupToDisappear(int timeoutSeconds = 10)
        {
            try
            {
                Logger.Info("等待结账弹出框消失");
                
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(driver => {
                    var popups = driver.FindElements(By.Id("checkOutPopUp"));
                    return popups.Count == 0 || popups.All(popup => !popup.Displayed);
                });
                
                Logger.Info("结账弹出框已消失");
            }
            catch (Exception ex)
            {
                Logger.Info($"等待结账弹出框消失超时: {ex.Message}");
            }
        }

        public bool AddToCart(string colorName, int quantity)
        {
            Logger.Info($"开始添加商品到购物车 - 颜色: {colorName}, 数量: {quantity}");
            
            // 1. 选择颜色
            SelectColor(colorName);
            
            // 2. 设置数量
            SetQuantity(quantity);

            // 3. 尝试3次点击添加到购物车按钮
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    Logger.Info($"第{attempt}次尝试点击添加到购物车按钮");
                    
                    By addToCartButton = By.Name("save_to_cart");
                    var addButton = FindElementWithWait(addToCartButton);
                    addButton.Click();
                    Logger.Info($"已点击添加到购物车按钮（第{attempt}次）");
                    
                    // 验证是否成功
                    bool isSuccessful = IsAddToCartSuccessful();
                    
                    if (isSuccessful)
                    {
                        Logger.Info($"第{attempt}次尝试成功，商品已添加到购物车");
                        WaitForCheckoutPopupToDisappear();
                        return true;
                    }
                    
                    Logger.Info($"第{attempt}次尝试失败，未检测到成功弹出框");
                    
                    if (attempt < 3)
                    {
                        Logger.Info("等待3秒后重试");
                        ForcedWait(3);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Info($"第{attempt}次尝试异常: {ex.Message}");
                    
                    if (attempt == 3)
                    {
                        Logger.Info("所有尝试均失败，抛出异常");
                        throw;
                    }
                    
                    Logger.Info("等待3秒后重试");
                    ForcedWait(3);
                }
            }
            
            Logger.Info("所有尝试均失败，添加商品到购物车失败");
            return false;
        }
    }
}
