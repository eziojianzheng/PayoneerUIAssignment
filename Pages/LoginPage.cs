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
           
            ExtentTest?.Info("开始登录操作");

            FindElementWithWait(menuUserField).Click();
            ExtentTest?.Info($"点击userLogin的Menum");

            FindElementWithWait(usernameField).SendKeys(username);
            ExtentTest?.Info($"输入用户名: {username}");

            FindElementWithWait(passwordField).SendKeys(password);
            ExtentTest?.Info("输入密码");

            WaitForLoaderToDisappear(30);
            FindElementWithWait(loginButton).Click();
            ExtentTest?.Info("点击登录按钮");

            Logger.Info($"用户 {username} 已登录");
        

            var homePage = new HomePage(Driver, ExtentTest);
            return homePage;
        }

    }
}
