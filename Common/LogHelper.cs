using AventStack.ExtentReports;

namespace PayoneerUIAssignment.Common
{
    /// <summary>
    /// Unified logging helper class to avoid duplicate logging
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// Logs information message to both NLog and ExtentReports
        /// </summary>
        public static void LogInfo(string message, ExtentTest extentTest = null)
        {
            Logger.Info(message);
            extentTest?.Info(message);
        }

        /// <summary>
        /// Logs success message to both NLog and ExtentReports
        /// </summary>
        public static void LogPass(string message, ExtentTest extentTest = null)
        {
            Logger.Info($"✓ {message}");
            extentTest?.Pass(message);
        }

        /// <summary>
        /// Logs error message to both NLog and ExtentReports
        /// </summary>
        public static void LogError(string message, System.Exception ex = null, ExtentTest extentTest = null)
        {
            if (ex != null)
            {
                Logger.Error(message, ex);
                extentTest?.Error($"{message}: {ex.Message}");
            }
            else
            {
                Logger.Error(message, new System.Exception(message));
                extentTest?.Error(message);
            }
        }

        /// <summary>
        /// Logs failure message to both NLog and ExtentReports
        /// </summary>
        public static void LogFail(string message, ExtentTest extentTest = null)
        {
            Logger.Info($"✗ {message}");
            extentTest?.Fail(message);
        }
    }
}