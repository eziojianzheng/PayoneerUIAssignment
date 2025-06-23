using System;
using System.Collections.Generic;
using System.Linq;
using AventStack.ExtentReports;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using PayoneerUIAssignment.Common;

namespace PayoneerUIAssignment.Pages
{
    /// <summary>
    /// Product information model for cart operations
    /// </summary>
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

    /// <summary>
    /// Shopping cart page object for cart management operations
    /// </summary>
    public class ShoppingCartPage : BasePage
    {
        private readonly By cartIcon = By.Id("menuCart");
        private readonly By cartItems = By.CssSelector("#shoppingCart table tbody tr[ng-repeat*='product in cart.productsInCart']");
        private readonly By deleteButton = By.CssSelector("a.remove.red.ng-scope[translate='REMOVE']");
        private readonly By emptyCartMessage = By.CssSelector("label.roboto-bold.ng-scope[translate='Your_shopping_cart_is_empty']");
        private readonly By continueShoppingButton = By.CssSelector("a.a-button.ng-scope[translate='CONTINUE_SHOPPING']");
        private readonly By totalPriceElement = By.CssSelector("#shoppingCart tfoot span.roboto-medium.ng-binding");
        
        public ShoppingCartPage(IWebDriver driver, ExtentTest extentTest = null) : base(driver, extentTest)
        {
        }

        /// <summary>
        /// Navigates to shopping cart page
        /// </summary>
        public ShoppingCartPage NavigateToCart()
        {
            try
            {
                LogHelper.LogInfo("Navigating to shopping cart page", ExtentTest);
                var cartElement = FindElementWithWait(cartIcon);
                cartElement.Click();
                
                // Wait for popup to disappear
                WaitForCartPopupToDisappear();
                WaitForPageFullyLoaded();
                LogHelper.LogInfo("Successfully entered shopping cart page", ExtentTest);
                return this;
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Navigation to shopping cart failed: {ex.Message}", ex, ExtentTest);
                throw;
            }
        }

        /// <summary>
        /// Checks if shopping cart has items
        /// </summary>
        public bool HasItems()
        {
            try
            {
                LogHelper.LogInfo("Checking if shopping cart has items", ExtentTest);
                var items = FindElementsWithoutWait(cartItems);
                bool hasItems = items.Count > 0;
                LogHelper.LogInfo($"Shopping cart item count: {items.Count}", ExtentTest);
                return hasItems;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"Shopping cart items check failed: {ex.Message}", ExtentTest);
                return false;
            }
        }

        /// <summary>
        /// Clears all items from shopping cart
        /// </summary>
        public void ClearAllItems()
        {
            try
            {
                LogHelper.LogInfo("Starting to clear shopping cart", ExtentTest);
                
                while (HasItems())
                {
                    var deleteButtons = FindElementsWithoutWait(deleteButton);
                    if (deleteButtons.Count > 0)
                    {
                        deleteButtons[0].Click();
                        WaitForPageFullyLoaded();
                        LogHelper.LogInfo("Deleted one item", ExtentTest);
                    }
                    else
                    {
                        break;
                    }
                }
                
                LogHelper.LogInfo("Shopping cart cleared", ExtentTest);
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Clear shopping cart failed: {ex.Message}", ex, ExtentTest);
                throw;
            }
        }

        /// <summary>
        /// Checks if shopping cart is empty
        /// </summary>
        public bool IsEmpty()
        {
            try
            {
                LogHelper.LogInfo("Verifying if shopping cart is empty", ExtentTest);
                
                // Check for empty cart message
                var emptyMessages = FindElementsWithoutWait(emptyCartMessage);
                if (emptyMessages.Count > 0 && emptyMessages[0].Displayed)
                {
                    string messageText = emptyMessages[0].Text;
                    LogHelper.LogInfo($"Found empty cart message: {messageText}", ExtentTest);
                    
                    if (messageText.Equals("Your shopping cart is empty", StringComparison.OrdinalIgnoreCase))
                    {
                        LogHelper.LogInfo("Cart is empty, clicking continue shopping button", ExtentTest);
                        ClickContinueShopping();
                        return true;
                    }
                }
                
                // If no empty cart message, check if there are items
                bool isEmpty = !HasItems();
                LogHelper.LogInfo($"Shopping cart status: {(isEmpty ? "Empty" : "Has items")}", ExtentTest);
                return isEmpty;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"Shopping cart status verification failed: {ex.Message}", ExtentTest);
                return false;
            }
        }

        /// <summary>
        /// Verifies cart products match expected products
        /// </summary>
        public bool VerifyCartProducts(ProductInfo[] expectedProducts)
        {
            try
            {
                LogHelper.LogInfo("Starting cart products verification", ExtentTest);
                
                var cartRows = FindElementsWithWait(cartItems);
                if (cartRows.Count != expectedProducts.Length)
                {
                    LogHelper.LogInfo($"Product count mismatch: Expected {expectedProducts.Length}, Actual {cartRows.Count}", ExtentTest);
                    return false;
                }
                
                // Get all product information from cart
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
                        
                        // Calculate price
                        var priceElement = row.FindElement(By.CssSelector("p.price.roboto-regular.ng-binding"));
                        string priceText = priceElement.Text.Replace("$", "").Replace(",", "");
                        if (decimal.TryParse(priceText, out decimal totalPrice))
                        {
                            decimal unitPrice = totalPrice / quantity;
                            calculatedTotal += totalPrice;
                            
                            actualProducts.Last().UnitPrice = unitPrice;
                            LogHelper.LogInfo($"Product: {name}, Unit Price: ${unitPrice:F2}, Quantity: {quantity}, Subtotal: ${totalPrice:F2}, Color: {color}", ExtentTest);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogInfo($"Extract product info failed: {ex.Message}", ExtentTest);
                        continue;
                    }
                }
                
                // Compare product information (order independent)
                foreach (var expected in expectedProducts)
                {
                    var match = actualProducts.FirstOrDefault(p => 
                        p.Name.Equals(expected.Name, StringComparison.OrdinalIgnoreCase) &&
                        p.Quantity == expected.Quantity &&
                        p.Color.Equals(expected.Color, StringComparison.OrdinalIgnoreCase));
                    
                    if (match == null)
                    {
                        LogHelper.LogInfo($"No matching product found: {expected.Name}, Quantity: {expected.Quantity}, Color: {expected.Color}", ExtentTest);
                        return false;
                    }
                }
                
                // Verify total price
                string actualTotalPrice = GetTotalPrice();
                if (!string.IsNullOrEmpty(actualTotalPrice))
                {
                    string totalPriceText = actualTotalPrice.Replace("$", "").Replace(",", "");
                    if (decimal.TryParse(totalPriceText, out decimal displayedTotal))
                    {
                        if (Math.Abs(calculatedTotal - displayedTotal) < 0.01m)
                        {
                            LogHelper.LogInfo($"Total price verification successful: Calculated ${calculatedTotal:F2}, Displayed {actualTotalPrice}", ExtentTest);
                        }
                        else
                        {
                            LogHelper.LogInfo($"Total price mismatch: Calculated ${calculatedTotal:F2}, Displayed {actualTotalPrice}", ExtentTest);
                            return false;
                        }
                    }
                }
                
                LogHelper.LogPass("All product information and price verification successful", ExtentTest);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Cart products verification failed: {ex.Message}", ex, ExtentTest);
                return false;
            }
        }

        /// <summary>
        /// Gets the total price from shopping cart
        /// </summary>
        public string GetTotalPrice()
        {
            try
            {
                LogHelper.LogInfo("Getting shopping cart total price", ExtentTest);
                var totalElement = FindElementWithWait(totalPriceElement);
                string totalPrice = totalElement.Text;
                LogHelper.LogInfo($"Shopping cart total price: {totalPrice}", ExtentTest);
                return totalPrice;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"Get total price failed: {ex.Message}", ExtentTest);
                return null;
            }
        }

        /// <summary>
        /// Waits for cart popup to disappear
        /// </summary>
        private void WaitForCartPopupToDisappear(int timeoutSeconds = 10)
        {
            try
            {
                LogHelper.LogInfo("Waiting for cart popup to disappear", ExtentTest);
                
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(driver => {
                    var popups = driver.FindElements(By.Id("mobileShoppingCart"));
                    return popups.Count == 0 || popups.All(popup => !popup.Displayed);
                });
                
                LogHelper.LogInfo("Cart popup disappeared", ExtentTest);
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"Cart popup wait timeout: {ex.Message}", ExtentTest);
            }
        }

        /// <summary>
        /// Clicks continue shopping button
        /// </summary>
        private void ClickContinueShopping()
        {
            try
            {
                LogHelper.LogInfo("Clicking continue shopping button", ExtentTest);
                var continueButton = FindElementWithWait(continueShoppingButton);
                continueButton.Click();
                WaitForPageFullyLoaded();
                LogHelper.LogInfo("Continue shopping clicked, returning to shopping page", ExtentTest);
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Continue shopping button click failed: {ex.Message}", ex, ExtentTest);
                throw;
            }
        }
    }
}
