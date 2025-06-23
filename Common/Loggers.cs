using System;

namespace PayoneerUIAssignment.Common
{
    /// <summary>
    /// Wrapper class for NLog logger functionality
    /// </summary>
    public class Logger
    {
        private static readonly NLog.Logger LoggerInstance = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Logs an informational message
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Info(string message)
        {
            LoggerInstance.Info(message);
        }

        /// <summary>
        /// Logs an error message with exception details
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="ex">The exception to log</param>
        public static void Error(string message, Exception ex)
        {
            LoggerInstance.Error(ex, message);
        }
    }
}