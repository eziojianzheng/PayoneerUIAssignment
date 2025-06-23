using System;
using System.Linq;
using AventStack.ExtentReports;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using PayoneerUIAssignment.Common;

namespace PayoneerUIAssignment.Pages
{
    /// <summary>
    /// Product page object for product details and cart operations
    /// </summary>
    public class ProductPage : BasePage
    {
        private readonly By productTitle = By.CssSelector("a.select.ng-binding");
        private readonly By quantityInput = By.Name("quantity");
        private readonly By addToCartButton = By.Name("save_to_cart");
        private readonly By checkoutPopup = By.Id("checkOutPopUp");
        
        public ProductPage(IWebDriver driver, ExtentTest extentTest = null) : base(driver, extentTest) { }

        /// <summary>
        /// Checks if product page is loaded successfully
        /// </summary>
        public bool IsProductPageLoaded()
        {
            try
            {
                LogHelper.LogInfo("Checking if product details page loaded successfully", ExtentTest);
                
                var titleElement = FindElementWithWait(productTitle, 10);
                bool isLoaded = titleElement != null && titleElement.Displayed;
                
                LogHelper.LogInfo($"Product details page load status: {(isLoaded ? "Success" : "Failed")}", ExtentTest);
                return isLoaded;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"Product details page check failed: {ex.Message}", ExtentTest);
                return false;
            }
        }

        /// <summary>
        /// Selects product color by name
        /// </summary>
        public void SelectColor(string colorName)
        {
            try
            {
                LogHelper.LogInfo($"Selecting color: {colorName}", ExtentTest);
                
                By colorLocator = By.CssSelector($"span.productColor[title='{colorName.ToUpper()}']");
                var colorElement = FindElementWithWait(colorLocator);
                colorElement.Click();
                
                LogHelper.LogInfo($"Color selected: {colorName}", ExtentTest);
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Color selection failed for {colorName}: {ex.Message}", ex, ExtentTest);
                throw;
            }
        }

        /// <summary>
        /// Sets product quantity
        /// </summary>
        public void SetQuantity(int quantity)
        {
            try
            {
                LogHelper.LogInfo($"Setting product quantity: {quantity}", ExtentTest);
                
                var quantityElement = FindElementWithWait(quantityInput);
                
                // Clear input field using multiple methods for reliability
                quantityElement.Click();
                quantityElement.SendKeys(Keys.Control + "a");
                quantityElement.SendKeys(Keys.Backspace);
                quantityElement.SendKeys(quantity.ToString());
                
                LogHelper.LogInfo($"Product quantity set: {quantity}", ExtentTest);
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Setting quantity {quantity} failed: {ex.Message}", ex, ExtentTest);
                throw;
            }
        }

        /// <summary>
        /// Checks if product was successfully added to cart by verifying popup
        /// </summary>
        private bool IsAddToCartSuccessful()
        {
            try
            {
                LogHelper.LogInfo("Checking if product was successfully added to cart", ExtentTest);
                
                var popupElement = FindElementWithWait(checkoutPopup, 5);
                bool isSuccessful = popupElement != null && popupElement.Displayed;
                
                LogHelper.LogInfo($"Add to cart status: {(isSuccessful ? "Success" : "Failed")}", ExtentTest);
                return isSuccessful;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"Add to cart status check failed: {ex.Message}", ExtentTest);
                return false;
            }
        }

        /// <summary>
        /// Waits for checkout popup to disappear
        /// </summary>
        private void WaitForCheckoutPopupToDisappear(int timeoutSeconds = 10)
        {
            LogHelper.LogInfo("Waiting for checkout popup to disappear", ExtentTest);
            
            bool disappeared = WaitForElementToDisappear(checkoutPopup, timeoutSeconds);
            
            if (disappeared)
            {
                LogHelper.LogInfo("Checkout popup disappeared", ExtentTest);
            }
            else
            {
                LogHelper.LogInfo("Checkout popup wait timeout", ExtentTest);
            }
        }

        /// <summary>
        /// Adds product to cart with specified color and quantity
        /// </summary>
        public bool AddToCart(string colorName, int quantity)
        {
            try
            {
                LogHelper.LogInfo($"Starting add to cart - Color: {colorName}, Quantity: {quantity}", ExtentTest);
                
                // 1. Select color
                SelectColor(colorName);
                
                // 2. Set quantity
                SetQuantity(quantity);

                // 3. Try adding to cart with retry mechanism
                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    try
                    {
                        LogHelper.LogInfo($"Attempt {attempt}: Clicking add to cart button", ExtentTest);
                        
                        var addButton = FindElementWithWait(addToCartButton);
                        addButton.Click();
                        LogHelper.LogInfo($"Add to cart button clicked (Attempt {attempt})", ExtentTest);
                        
                        // Verify success
                        if (IsAddToCartSuccessful())
                        {
                            LogHelper.LogPass($"Attempt {attempt} successful - Product added to cart", ExtentTest);
                            WaitForCheckoutPopupToDisappear();
                            return true;
                        }
                        
                        LogHelper.LogInfo($"Attempt {attempt} failed - Success popup not detected", ExtentTest);
                        
                        if (attempt < 3)
                        {
                            LogHelper.LogInfo("Waiting 3 seconds before retry", ExtentTest);
                            ForcedWait(3);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogInfo($"Attempt {attempt} exception: {ex.Message}", ExtentTest);
                        
                        if (attempt == 3)
                        {
                            LogHelper.LogError("All attempts failed, throwing exception", ex, ExtentTest);
                            throw;
                        }
                        
                        LogHelper.LogInfo("Waiting 3 seconds before retry", ExtentTest);
                        ForcedWait(3);
                    }
                }
                
                LogHelper.LogFail("All attempts failed - Add to cart unsuccessful", ExtentTest);
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Add to cart failed: {ex.Message}", ex, ExtentTest);
                throw;
            }
        }
    }
}
