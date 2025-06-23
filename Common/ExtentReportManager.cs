using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Configuration;
using System;
using System.IO;

namespace PayoneerUIAssignment.Common
{
    /// <summary>
    /// Factory class for creating and managing HTML test reports
    /// </summary>
    public class ExtentReportManager
    {
        private static readonly Lazy<ExtentReports> _instance = new Lazy<ExtentReports>(() => InitializeReport());
        private static readonly object _lock = new object();

        private static ExtentReports Instance => _instance.Value;

        private static string ReportPath => Path.Combine(Directory.GetCurrentDirectory(), "Reports");

        /// <summary>
        /// Initializes and configures the ExtentReports instance with HTML reporter
        /// </summary>
        /// <returns>Configured ExtentReports instance</returns>
        private static ExtentReports InitializeReport()
        {
            if (!Directory.Exists(ReportPath))
            {
                Directory.CreateDirectory(ReportPath);
            }

            var htmlReporter = new ExtentHtmlReporter(Path.Combine(ReportPath, $"TestReport_{DateTime.Now:yyyyMMdd_HHmmss}.html"));
            htmlReporter.Config.Theme = Theme.Standard;
            htmlReporter.Config.DocumentTitle = "UI Automation Test Report";
            htmlReporter.Config.ReportName = "Test Execution Results";
            htmlReporter.Config.EnableTimeline = true;

            var extentReports = new ExtentReports();
            extentReports.AttachReporter(htmlReporter);
            extentReports.AddSystemInfo("Operating System", Environment.OSVersion.ToString());
            extentReports.AddSystemInfo("Host Name", Environment.MachineName);
            extentReports.AddSystemInfo(".NET Version", Environment.Version.ToString());
            
            return extentReports;
        }

        /// <summary>
        /// Gets the singleton ExtentReports instance
        /// </summary>
        /// <returns>ExtentReports singleton instance</returns>
        public static ExtentReports CreateInstance()
        {
            return Instance;
        }

        /// <summary>
        /// Creates a new test entry in the report
        /// </summary>
        /// <param name="testName">Name of the test</param>
        /// <param name="description">Optional test description</param>
        /// <returns>ExtentTest instance for logging test steps</returns>
        public static ExtentTest CreateTest(string testName, string description = "")
        {
            return Instance.CreateTest(testName, description);
        }

        /// <summary>
        /// Flushes the report data to file in a thread-safe manner
        /// </summary>
        public static void Flush()
        {
            lock (_lock)
            {
                if (_instance.IsValueCreated)
                {
                    Instance.Flush();
                }
            }
        }
    }
}
