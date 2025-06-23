using System;
using System.Collections.Generic;
using System.Linq;
using AventStack.ExtentReports;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using PayoneerUIAssignment.Pages;
using PayoneerUIAssignment.Common;

namespace PayoneerUIAssignment.Pages
{
    public class ProductInfo
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Color { get; set; }
        public decimal UnitPrice { get; set; }
        
        public ProductInfo(string name, int quantity, string color, decimal unitPrice = 0)
        {
            Name = name;
            Quantity = quantity;
            Color = color;
            UnitPrice = unitPrice;
        }
    }

    public class ShoppingCartPage : BasePage
    {
        private readonly By cartIcon = By.Id("menuCart");
        private readonly By cartItems = By.CssSelector("#shoppingCart table tbody tr[ng-repeat*='product in cart.productsInCart']");
        private readonly By deleteButton = By.CssSelector("a.remove.red.ng-scope[translate='REMOVE']");
        private readonly By emptyCartMessage = By.CssSelector("label.roboto-bold.ng-scope[translate='Your_shopping_cart_is_empty']");
        private readonly By continueShoppingButton = By.CssSelector("a.a-button.ng-scope[translate='CONTINUE_SHOPPING']");
        
        public ShoppingCartPage(IWebDriver driver, ExtentTest extentTest = null) : base(driver, extentTest)
        {
        }

        public ShoppingCartPage NavigateToCart()
        {
            try
            {
                LogHelper.LogInfo("导航到购物车页面", ExtentTest);
                var cartElement = FindElementWithWait(cartIcon);
                cartElement.Click();
                
                // 等待弹出框消失
                WaitForCartPopupToDisappear();
                WaitForPageFullyLoaded();
                LogHelper.LogInfo("已进入购物车页面", ExtentTest);
                return this;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"导航到购物车失败: {ex.Message}", ExtentTest);
                throw;
            }
        }

        public bool HasItems()
        {
            try
            {
                LogHelper.LogInfo("检查购物车是否有商品", ExtentTest);
                var items = FindElementsWithoutWait(cartItems);
                bool hasItems = items.Count > 0;
                LogHelper.LogInfo($"购物车商品数量: {items.Count}", ExtentTest);
                return hasItems;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"检查购物车商品失败: {ex.Message}", ExtentTest);
                return false;
            }
        }

        public void ClearAllItems()
        {
            try
            {
                LogHelper.LogInfo("开始清空购物车", ExtentTest);
                
                while (HasItems())
                {
                    var deleteButtons = FindElementsWithoutWait(deleteButton);
                    if (deleteButtons.Count > 0)
                    {
                        deleteButtons[0].Click();
                        WaitForPageFullyLoaded();
                        LogHelper.LogInfo("已删除一个商品", ExtentTest);
                    }
                    else
                    {
                        break;
                    }
                }
                
                LogHelper.LogInfo("购物车已清空", ExtentTest);
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"清空购物车失败: {ex.Message}", ExtentTest);
                throw;
            }
        }

        public bool IsEmpty()
        {
            try
            {
                LogHelper.LogInfo("验证购物车是否为空", ExtentTest);
                
                // 检查空购物车消息
                var emptyMessages = FindElementsWithoutWait(emptyCartMessage);
                if (emptyMessages.Count > 0 && emptyMessages[0].Displayed)
                {
                    string messageText = emptyMessages[0].Text;
                    LogHelper.LogInfo($"找到空购物车消息: {messageText}", ExtentTest);
                    
                    if (messageText.Equals("Your shopping cart is empty", StringComparison.OrdinalIgnoreCase))
                    {
                        LogHelper.LogInfo("购物车为空，点击继续购物按钮", ExtentTest);
                        ClickContinueShopping();
                        return true;
                    }
                }
                
                // 如果没有空购物车消息，检查是否有商品
                bool isEmpty = !HasItems();
                LogHelper.LogInfo($"购物车状态: {(isEmpty ? "空" : "有商品")}", ExtentTest);
                return isEmpty;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"验证购物车状态失败: {ex.Message}", ExtentTest);
                return false;
            }
        }

        public bool VerifyCartProducts(ProductInfo[] expectedProducts)
        {
            try
            {
                LogHelper.LogInfo("开始验证购物车商品信息", ExtentTest);
                
                var cartRows = FindElementsWithWait(cartItems);
                if (cartRows.Count != expectedProducts.Length)
                {
                    LogHelper.LogInfo($"商品数量不匹配: 期望{expectedProducts.Length}个，实际{cartRows.Count}个", ExtentTest);
                    return false;
                }
                
                // 获取购物车中的所有商品信息
                var actualProducts = new List<ProductInfo>();
                decimal calculatedTotal = 0;
                
                foreach (var row in cartRows)
                {
                    try
                    {
                        var nameElement = row.FindElement(By.CssSelector("label.roboto-regular.productName.ng-binding"));
                        string name = nameElement.Text;
                        
                        var quantityElement = row.FindElement(By.CssSelector("td.smollCell.quantityMobile label.ng-binding"));
                        int quantity = int.Parse(quantityElement.Text);
                        
                        var colorElement = row.FindElement(By.CssSelector("span.productColor"));
                        string color = colorElement.GetAttribute("title");
                        
                        actualProducts.Add(new ProductInfo(name, quantity, color));
                        
                        // 计算价格
                        var priceElement = row.FindElement(By.CssSelector("p.price.roboto-regular.ng-binding"));
                        string priceText = priceElement.Text.Replace("$", "").Replace(",", "");
                        if (decimal.TryParse(priceText, out decimal totalPrice))
                        {
                            decimal unitPrice = totalPrice / quantity;
                            calculatedTotal += totalPrice;
                            
                            actualProducts.Last().UnitPrice = unitPrice;
                            LogHelper.LogInfo($"商品: {name}, 单价: ${unitPrice:F2}, 数量: {quantity}, 小计: ${totalPrice:F2}, 颜色: {color}", ExtentTest);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogInfo($"提取商品信息失败: {ex.Message}", ExtentTest);
                        continue;
                    }
                }
                
                // 比较商品信息（不考虑顺序）
                foreach (var expected in expectedProducts)
                {
                    var match = actualProducts.FirstOrDefault(p => 
                        p.Name.Equals(expected.Name, StringComparison.OrdinalIgnoreCase) &&
                        p.Quantity == expected.Quantity &&
                        p.Color.Equals(expected.Color, StringComparison.OrdinalIgnoreCase));
                    
                    if (match == null)
                    {
                        LogHelper.LogInfo($"未找到匹配的商品: {expected.Name}, 数量: {expected.Quantity}, 颜色: {expected.Color}", ExtentTest);
                        return false;
                    }
                }
                
                // 验证总价格
                string actualTotalPrice = GetTotalPrice();
                if (!string.IsNullOrEmpty(actualTotalPrice))
                {
                    string totalPriceText = actualTotalPrice.Replace("$", "").Replace(",", "");
                    if (decimal.TryParse(totalPriceText, out decimal displayedTotal))
                    {
                        if (Math.Abs(calculatedTotal - displayedTotal) < 0.01m)
                        {
                            LogHelper.LogInfo($"总价格验证成功: 计算总价${calculatedTotal:F2}, 显示总价{actualTotalPrice}", ExtentTest);
                        }
                        else
                        {
                            LogHelper.LogInfo($"总价格不匹配: 计算总价${calculatedTotal:F2}, 显示总价{actualTotalPrice}", ExtentTest);
                            return false;
                        }
                    }
                }
                
                LogHelper.LogInfo("所有商品信息和价格验证成功", ExtentTest);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"验证购物车商品失败: {ex.Message}", ExtentTest);
                return false;
            }
        }

        public string GetTotalPrice()
        {
            try
            {
                LogHelper.LogInfo("获取购物车总价格", ExtentTest);
                var totalPriceElement = FindElementWithWait(By.CssSelector("#shoppingCart tfoot span.roboto-medium.ng-binding"));
                string totalPrice = totalPriceElement.Text;
                LogHelper.LogInfo($"购物车总价格: {totalPrice}", ExtentTest);
                return totalPrice;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"获取总价格失败: {ex.Message}", ExtentTest);
                return null;
            }
        }

        private void WaitForCartPopupToDisappear(int timeoutSeconds = 10)
        {
            try
            {
                LogHelper.LogInfo("等待购物车弹出框消失", ExtentTest);
                
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(driver => {
                    var popups = driver.FindElements(By.Id("mobileShoppingCart"));
                    return popups.Count == 0 || popups.All(popup => !popup.Displayed);
                });
                
                LogHelper.LogInfo("购物车弹出框已消失", ExtentTest);
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"等待购物车弹出框消失超时: {ex.Message}", ExtentTest);
            }
        }

        private void ClickContinueShopping()
        {
            try
            {
                LogHelper.LogInfo("点击继续购物按钮", ExtentTest);
                var continueButton = FindElementWithWait(continueShoppingButton);
                continueButton.Click();
                WaitForPageFullyLoaded();
                LogHelper.LogInfo("已点击继续购物，返回购物页面", ExtentTest);
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"点击继续购物按钮失败: {ex.Message}", ExtentTest);
                throw;
            }
        }
    }
}
