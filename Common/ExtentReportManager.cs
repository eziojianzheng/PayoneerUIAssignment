using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Configuration;
using System;
using System.IO;

namespace PayoneerUIAssignment.Common
{
    public class ExtentReportManager
    {
        private static readonly Lazy<ExtentReports> _instance = new Lazy<ExtentReports>(() => InitializeReport());
        private static bool _isInitialized = false;

        private static ExtentReports Instance => _instance.Value;

        private static string ReportPath => Path.Combine(Directory.GetCurrentDirectory(), "Reports");

        private static ExtentReports InitializeReport()
        {
            if (!Directory.Exists(ReportPath))
            {
                Directory.CreateDirectory(ReportPath);
            }

            var htmlReporter = new ExtentHtmlReporter(Path.Combine(ReportPath, $"TestReport_{DateTime.Now:yyyyMMdd_HHmmss}.html"));
            htmlReporter.Config.Theme = Theme.Standard;
            htmlReporter.Config.DocumentTitle = "UI自动化测试报告";
            htmlReporter.Config.ReportName = "测试执行结果";
            htmlReporter.Config.EnableTimeline = true;

            var extentReports = new ExtentReports();
            extentReports.AttachReporter(htmlReporter);
            extentReports.AddSystemInfo("操作系统", Environment.OSVersion.ToString());
            extentReports.AddSystemInfo("主机名", Environment.MachineName);
            extentReports.AddSystemInfo(".NET版本", Environment.Version.ToString());
            
            _isInitialized = true;
            return extentReports;
        }

        public static ExtentReports CreateInstance()
        {
            return Instance;
        }

        public static ExtentTest CreateTest(string testName, string description = "")
        {
            return Instance.CreateTest(testName, description);
        }

        public static void Flush()
        {
            Instance.Flush();
        }
    }
}
