# PayoneerUIAssignment - UI Test Automation Framework

## Overview
A comprehensive UI test automation framework built with Selenium WebDriver and Page Object Model pattern for end-to-end testing of e-commerce applications.

## Framework Capabilities

### 🚀 **Core Features**
- **Page Object Model (POM)** - Maintainable and reusable page components
- **Smart Element Waiting** - Intelligent waits for elements and page loading
- **Unified Logging** - Dual logging to NLog and ExtentReports
- **Screenshot Management** - Automatic screenshots on failures and key steps
- **Configuration Management** - Flexible configuration with environment variable support
- **Cross-Browser Support** - Chrome browser with headless mode option

### 📊 **Reporting & Logging**
- **ExtentReports** - Rich HTML test reports with screenshots
- **NLog Integration** - Structured logging with daily rotation
- **Real-time Logging** - Synchronized logging between file and reports
- **Visual Evidence** - Automatic screenshot capture for test steps

### 🛠 **Test Infrastructure**
- **TestBase Class** - Common test setup and teardown
- **LogHelper** - Unified logging interface
- **DriverFactory** - Browser driver management
- **Common Test Methods** - Reusable login and cart cleanup methods

## Technology Stack
- **.NET 8.0** - Modern .NET framework
- **Selenium WebDriver 4.33** - Browser automation
- **MSTest** - Test framework
- **ExtentReports 4.1** - Test reporting
- **NLog 5.5** - Logging framework
- **WebDriverManager** - Automatic driver management

## Project Structure
```
├── Common/
│   ├── TestBase.cs          # Base test class with common methods
│   ├── LogHelper.cs         # Unified logging helper
│   ├── DriverFactory.cs     # Browser driver factory
│   └── ExtentReportManager.cs # Report management
├── Pages/
│   ├── BasePage.cs          # Base page with common page methods
│   ├── LoginPage.cs         # Login page object
│   ├── HomePage.cs          # Home page with search functionality
│   ├── ProductPage.cs       # Product details and cart operations
│   └── ShoppingCartPage.cs  # Shopping cart management
├── Tests/
│   └── AddToCartETE_Test.cs # End-to-end shopping tests
├── Config/
│   └── appsettings.json     # Test configuration
└── NLog.config              # Logging configuration
```

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Chrome Browser
- Visual Studio 2022 or VS Code

### Installation & Execution
```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run all tests
dotnet test

# Run with specific browser (optional)
dotnet test --logger "console;verbosity=detailed" -- TestRunParameters.Parameter(name="Browser", value="Chrome")
```

### Configuration Options

#### appsettings.json
```json
{
  "BaseUrl": "https://www.advantageonlineshopping.com/",
  "Browser": "Chrome",
  "ValidUser": {
    "Username": "your_username",
    "Password": "your_password"
  }
}
```

#### Environment Variables
- `Browser` - Override browser selection
- `HeadlessMode` - Set to "true" for headless execution

## Page Objects

### 🏠 **LoginPage**
Handles user authentication with credential validation.
```csharp
var loginPage = new LoginPage(driver, extentTest);
var homePage = loginPage.Login("username", "password");
```

### 🏪 **HomePage** 
Provides search functionality and navigation to other pages.
```csharp
var homePage = new HomePage(driver, extentTest);
var searchResults = homePage.SearchProduct("HP Mouse");
var cartPage = homePage.NavigateToShoppingCart();
```

### 📦 **ProductPage**
Manages product selection, color/quantity options, and cart operations.
```csharp
var productPage = new ProductPage(driver, extentTest);
bool success = productPage.AddToCart("Black", 2);
```

### 🛒 **ShoppingCartPage**
Handles cart verification, item management, and price calculations.
```csharp
var cartPage = new ShoppingCartPage(driver, extentTest);
bool isValid = cartPage.VerifyCartProducts(expectedProducts);
string totalPrice = cartPage.GetTotalPrice();
```

## Usage Examples

### Basic Test Flow
```csharp
[TestMethod]
public void Shopping_Test()
{
    // Step 1: Login
    var homePage = LoginAndVerify(username, password);
    
    // Step 2: Clear cart
    CheckAndClearShoppingCart(homePage);
    
    // Step 3: Search and add product
    var searchResults = homePage.SearchProduct("HP Mouse");
    var productPage = searchResults.SelectFirstProduct();
    productPage.AddToCart("Black", 1);
    
    // Step 4: Verify cart
    var cartPage = homePage.NavigateToShoppingCart();
    Assert.IsTrue(cartPage.VerifyCartProducts(expectedProducts));
}
```

## Reports & Artifacts
- **HTML Reports** - `Reports/TestReport_[timestamp].html`
- **Screenshots** - `Screenshots/[step]_[timestamp].png` 
- **Log Files** - `Logs/log-[date].log`