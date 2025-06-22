using System;

namespace PayoneerUIAssignment.Common
{
    public class Logger
    {
        private static readonly NLog.Logger LoggerInstance = NLog.LogManager.GetCurrentClassLogger();

        public static void Info(string message)
        {
            LoggerInstance.Info(message);
        }

        public static void Error(string message, Exception ex)
        {
            LoggerInstance.Error(ex, message);
        }
    }
} 