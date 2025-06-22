# PayoneerUIAssignment

## 项目描述
UI自动化测试框架，基于Selenium WebDriver和Page Object Model模式，用于电商网站的端到端测试。

## 技术栈
- .NET 8.0
- Selenium WebDriver
- MSTest
- ExtentReports (测试报告)
- NLog (日志记录)

## 项目结构
```
├── Common/          # 公共类库
├── Config/          # 配置文件
├── Pages/           # 页面对象模型
├── Tests/           # 测试用例
└── README.md
```

## 快速开始

### 1. 环境要求
- .NET 8.0 SDK
- Chrome浏览器
- Visual Studio 2022 或 VS Code

### 2. 运行测试
```bash
# 还原依赖
dotnet restore

# 构建项目
dotnet build

# 运行测试
dotnet test
```

### 3. 配置说明
- `Config/appsettings.json`: 测试配置
- `NLog.config`: 日志配置
- `test.runsettings`: 测试运行设置

## 测试用例
- **AddToCart E2E Test**: 完整的购物车添加流程测试
  - 用户登录
  - 商品搜索
  - 添加到购物车
  - 购物车验证

## 测试报告
测试完成后会生成：
- ExtentReports HTML报告
- 测试截图
- 详细日志

