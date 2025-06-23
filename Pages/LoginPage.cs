using AventStack.ExtentReports;
using OpenQA.Selenium;
using PayoneerUIAssignment.Common;
using System;

namespace PayoneerUIAssignment.Pages
{
    /// <summary>
    /// Login page object for user authentication
    /// </summary>
    public class LoginPage : BasePage
    {
        private readonly By menuUserField = By.Id("menuUser");
        private readonly By usernameField = By.Name("username");
        private readonly By passwordField = By.Name("password");
        private readonly By loginButton = By.Id("sign_in_btn");

        public LoginPage(IWebDriver driver, ExtentTest extentTest = null) : base(driver, extentTest) { }

        /// <summary>
        /// Performs user login with provided credentials
        /// </summary>
        /// <param name="username">Username for login</param>
        /// <param name="password">Password for login</param>
        /// <returns>HomePage instance after successful login</returns>
        public HomePage Login(string username, string password)
        {
            try
            {
                LogHelper.LogInfo("Starting login operation", ExtentTest);

                // Click user menu to open login form
                FindElementWithWait(menuUserField).Click();
                LogHelper.LogInfo("Clicked user login menu", ExtentTest);

                // Enter username
                var usernameElement = FindElementWithWait(usernameField);
                usernameElement.Clear();
                usernameElement.SendKeys(username);
                LogHelper.LogInfo($"Entered username: {username}", ExtentTest);

                // Enter password
                var passwordElement = FindElementWithWait(passwordField);
                passwordElement.Clear();
                passwordElement.SendKeys(password);
                LogHelper.LogInfo("Entered password", ExtentTest);

                // Wait for any loaders and click login button
                WaitForLoaderToDisappear(30);
                FindElementWithWait(loginButton).Click();
                LogHelper.LogInfo("Clicked login button", ExtentTest);

                // Wait for login to complete
                WaitForLoaderToDisappear(30);
                WaitForPageFullyLoaded();
                
                LogHelper.LogPass($"User {username} logged in successfully", ExtentTest);
                return new HomePage(Driver, ExtentTest);
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Login failed for user {username}: {ex.Message}", ex, ExtentTest);
                throw;
            }
        }

        /// <summary>
        /// Checks if login page is loaded by verifying presence of login elements
        /// </summary>
        public bool IsLoginPageLoaded()
        {
            try
            {
                var menuUser = FindElementWithWait(menuUserField, 5);
                bool isLoaded = menuUser != null && menuUser.Displayed;
                LogHelper.LogInfo($"Login page loaded: {isLoaded}", ExtentTest);
                return isLoaded;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo($"Login page check failed: {ex.Message}", ExtentTest);
                return false;
            }
        }
    }
}
