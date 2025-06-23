using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using PayoneerUIAssignment.Common;
using PayoneerUIAssignment.Pages;

namespace PayoneerUIAssignment.Tests
{
    /// <summary>
    /// Product information model for test data
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
    /// End-to-end shopping cart tests
    /// </summary>
    [TestClass]
    public class ShoppingETE_Tests : TestBase
    {
        /// <summary>
        /// Tests searching and adding multiple products to shopping cart
        /// </summary>
        [TestMethod]
        public void Search_Multiple_Products_Test()
        {
            // Arrange
            string username = Config["ValidUser:Username"];
            string password = Config["ValidUser:Password"];
            var products = new ProductInfo[]
            {
                new ProductInfo("HP Z8000 BLUETOOTH MOUSE", 1, "Black", 50.99m),
                new ProductInfo("HP ZBOOK 17 G2 MOBILE WORKSTATION", 2, "gray", 1799m),
                new ProductInfo("HP ELITE X2 1011 G1 TABLET", 1, "Black", 1279m)
            };
            
            LogHelper.LogInfo("Starting multiple products search test", ExtentTest);

            // Act & Assert
            // Step 1: Login and verify
            var homePage = LoginAndVerify(username, password);
            
            // Step 2: Check and clear shopping cart
            CheckAndClearShoppingCart(homePage);
            
            // Step 3: Search and add products
            SearchAndViewProducts(homePage, products);
            
            // Step 4: Verify shopping cart
            VerifyShoppingCart(homePage, products);
            
            LogHelper.LogPass("Multiple products search test completed successfully", ExtentTest);
        }


        
        /// <summary>
        /// Step 3: Search and add products to shopping cart
        /// </summary>
        private void SearchAndViewProducts(HomePage homePage, ProductInfo[] products)
        {
            LogHelper.LogInfo("Step 3: Starting product search and add to cart", ExtentTest);
            
            foreach (var product in products)
            {
                LogHelper.LogInfo($"Processing product: {product.Name}, Quantity: {product.Quantity}, Color: {product.Color}", ExtentTest);
                
                // 1. Search product
                var searchCategories = homePage.SearchProduct(product.Name);
                Assert.IsTrue(searchCategories.HasSearchResults(product.Name), $"Should have search results for {product.Name}");
                LogHelper.LogInfo($"{product.Name} search results verification successful", ExtentTest);
                
                // 2. Enter product details page
                var productPage = searchCategories.SelectFirstProduct();
                Assert.IsTrue(productPage.IsProductPageLoaded(), "Should successfully enter product details page");
                LogHelper.LogInfo($"Successfully entered {product.Name} product details page", ExtentTest);
                
                // 3. Add to cart
                bool addSuccess = productPage.AddToCart(product.Color, product.Quantity);
                Assert.IsTrue(addSuccess, $"Product {product.Name} should be successfully added to cart");
                LogHelper.LogPass($"Successfully added {product.Quantity} {product.Color} {product.Name} to cart", ExtentTest);
            }
            
            LogHelper.LogPass("Step 3 completed: All products successfully added to cart", ExtentTest);
        }
        
        /// <summary>
        /// Step 4: Verify shopping cart product information
        /// </summary>
        private void VerifyShoppingCart(HomePage homePage, ProductInfo[] expectedProducts)
        {
            LogHelper.LogInfo("Step 4: Verifying shopping cart product information", ExtentTest);
            
            var cartPage = homePage.NavigateToShoppingCart();
            
            // Verify product information
            var cartProducts = expectedProducts.Select(p => new PayoneerUIAssignment.Pages.ProductInfo(p.Name, p.Quantity, p.Color)).ToArray();
            bool isValid = cartPage.VerifyCartProducts(cartProducts);
            Assert.IsTrue(isValid, "Shopping cart product information should match expectations");
            LogHelper.LogPass("Shopping cart product information verification successful", ExtentTest);
            
            // Get and verify total price
            string totalPrice = cartPage.GetTotalPrice();
            Assert.IsNotNull(totalPrice, "Should be able to get total price");
            LogHelper.LogInfo($"Shopping cart total price: {totalPrice}", ExtentTest);  
            LogHelper.LogPass("Step 4 completed: Shopping cart information verification successful", ExtentTest);

        }
    }
}
