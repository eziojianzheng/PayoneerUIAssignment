using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using PayoneerUIAssignment.Common;
using PayoneerUIAssignment.Pages;
using static PayoneerUIAssignment.Pages.HomePage;
using PayoneerUIAssignment.Pages;

// 注意：以下页面类需要根据实际情况创建：
// - SearchPage, ProductPage, CartPage, CheckoutPage, OrderPage

namespace PayoneerUIAssignment.Tests
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
    [TestClass]
    public class ShoppingETE_Tests : TestBase
    {
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
            
            LogHelper.LogInfo("开始执行多个商品搜索测试", ExtentTest);

            // Act & Assert
            var homePage = LoginAndVerify(username, password);
            CheckAndClearShoppingCart(homePage);
            SearchAndViewProducts(homePage, products);
            VerifyShoppingCart(homePage, products);
            
            LogHelper.LogPass("多个商品搜索测试成功完成", ExtentTest);
        }

        // Step 1: 登录并验证
        private HomePage LoginAndVerify(string username, string password)
        {
            LogHelper.LogInfo("步骤1: 开始登录流程", ExtentTest);
            
            var loginPage = new LoginPage(Driver, ExtentTest).NavigateTo<LoginPage>(Config["BaseUrl"]);
            TakeScreenshot("login-page");
 
            var homePage = loginPage.Login(username, password);
           
            bool isLoggedIn = homePage.IsLoggedIn(username);
            Assert.IsTrue(isLoggedIn, $"用户 {username} 应该成功登录");
            LogHelper.LogPass($"步骤1完成: 用户 {username} 成功登录", ExtentTest);
            
            TakeScreenshot("home-page");
           
            return homePage;
        }
        
        // Step 2: 检查并清理购物车
        private void CheckAndClearShoppingCart(HomePage homePage)
        {
            LogHelper.LogInfo("步骤2: 检查并清理购物车", ExtentTest);
            
            var cartPage = homePage.NavigateToShoppingCart();
            
            if (cartPage.HasItems())
            {
                LogHelper.LogInfo("购物车中有商品，开始清理", ExtentTest);
                cartPage.ClearAllItems();
                LogHelper.LogPass("购物车已清空", ExtentTest);
            }
            else
            {
                LogHelper.LogInfo("购物车为空，无需清理", ExtentTest);
            }
            
            // 验证购物车为空并返回主页
            bool isEmpty = cartPage.IsEmpty();
            Assert.IsTrue(isEmpty, "购物车应该为空");
            LogHelper.LogPass("步骤2完成: 购物车已清理并返回主页", ExtentTest);
            
            TakeScreenshot("shopping-cart-cleared");
        }
        
        // Step 3: 搜索并添加商品到购物车
        private void SearchAndViewProducts(HomePage homePage, ProductInfo[] products)
        {
            LogHelper.LogInfo("步骤3: 开始搜索并添加商品到购物车", ExtentTest);
            
            foreach (var product in products)
            {
                LogHelper.LogInfo($"处理商品: {product.Name}, 数量: {product.Quantity}, 颜色: {product.Color}", ExtentTest);
                
                // 1. 搜索商品
                var searchCategories = homePage.SearchProduct(product.Name);
                Assert.IsTrue(searchCategories.HasSearchResults(product.Name), $"应该有{product.Name}搜索结果");
                LogHelper.LogInfo($"{product.Name}搜索结果验证成功", ExtentTest);
                
                TakeScreenshot($"search-results-{product.Name.Replace(" ", "-").ToLower()}");
                
                // 2. 进入商品详情页
                var productPage = searchCategories.SelectFirstProduct();
                Assert.IsTrue(productPage.IsProductPageLoaded(), "应该成功进入商品详情页");
                LogHelper.LogInfo($"成功进入{product.Name}商品详情页", ExtentTest);
                
                TakeScreenshot($"product-page-{product.Name.Replace(" ", "-").ToLower()}");
                
                // 3. 添加到购物车
                bool addSuccess = productPage.AddToCart(product.Color, product.Quantity);
                Assert.IsTrue(addSuccess, $"商品 {product.Name} 应该成功添加到购物车");
                LogHelper.LogPass($"成功添加 {product.Quantity} 个 {product.Color} 颜色的 {product.Name} 到购物车", ExtentTest);
                
                TakeScreenshot($"add-to-cart-success-{product.Name.Replace(" ", "-").ToLower()}");
            }
            
            LogHelper.LogPass("步骤3完成: 所有商品成功添加到购物车", ExtentTest);
        }
        
        // Step 4: 验证购物车商品信息
        private void VerifyShoppingCart(HomePage homePage, ProductInfo[] expectedProducts)
        {
            LogHelper.LogInfo("步骤4: 验证购物车商品信息", ExtentTest);
            
            var cartPage = homePage.NavigateToShoppingCart();
            
            // 验证商品信息
            var cartProducts = expectedProducts.Select(p => new PayoneerUIAssignment.Pages.ProductInfo(p.Name, p.Quantity, p.Color)).ToArray();
            bool isValid = cartPage.VerifyCartProducts(cartProducts);
            Assert.IsTrue(isValid, "购物车中的商品信息应该与期望一致");
            LogHelper.LogPass("购物车商品信息验证成功", ExtentTest);
            
            // 获取并验证总价格
            string totalPrice = cartPage.GetTotalPrice();
            Assert.IsNotNull(totalPrice, "应该能获取到总价格");
            LogHelper.LogInfo($"购物车总价格: {totalPrice}", ExtentTest);
            
            LogHelper.LogPass("步骤4完成: 购物车信息验证成功", ExtentTest);
            
            TakeScreenshot("shopping-cart-verified");
        }
        /*
        
        // Step 3: 结账并验证
        private OrderPage CheckoutAndVerify(CartPage cartPage)
        {
            Logger.Info("步骤3: 开始结账流程");
            ExtentTest.Info("步骤3: 开始结账流程");
            
            var checkoutPage = cartPage.Checkout();
            var orderPage = checkoutPage.CompleteOrder();
            
            Assert.IsTrue(orderPage.IsOrderComplete(), "订单应该完成");
            ExtentTest.Pass("步骤3完成: 订单成功完成");
            
            TakeScreenshot("order-complete");
            return orderPage;
        }
        */
    }
}
