using AventStack.ExtentReports;
using OpenQA.Selenium;
using PayoneerUIAssignment.Common;

namespace PayoneerUIAssignment.Pages
{
    public class LoginPage : BasePage
    {
        private readonly By menuUserField = By.Id("menuUser");
        private readonly By usernameField = By.Name("username");
        private readonly By passwordField = By.Name("password");
        private readonly By loginButton = By.Id("sign_in_btn");
        private readonly By loader = By.CssSelector("div.loader");

        public LoginPage(IWebDriver driver, ExtentTest extentTest = null) : base(driver, extentTest) { }

        public HomePage Login(string username, string password)
        {
           
            LogHelper.LogInfo("开始登录操作", ExtentTest);

            FindElementWithWait(menuUserField).Click();
            LogHelper.LogInfo($"点击userLogin的Menum", ExtentTest);

            FindElementWithWait(usernameField).SendKeys(username);
            LogHelper.LogInfo($"输入用户名: {username}", ExtentTest);

            FindElementWithWait(passwordField).SendKeys(password);
            LogHelper.LogInfo("输入密码", ExtentTest);

            WaitForLoaderToDisappear(30);
            FindElementWithWait(loginButton).Click();
            LogHelper.LogInfo("点击登录按钮", ExtentTest);

            LogHelper.LogInfo($"用户 {username} 已登录", ExtentTest);
        

            var homePage = new HomePage(Driver, ExtentTest);
            return homePage;
        }

    }
}
