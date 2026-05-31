using System;
using System.Drawing;

namespace LegoTrainProject.Logging
{
    /// <summary>
    /// Logging severity levels.
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Fatal = 4
    }

    /// <summary>
    /// Interface for centralized logging throughout the application.
    /// Abstracts away the UI dependency for testability.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a message with the specified level.
        /// </summary>
        void Log(LogLevel level, string message);

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        void Debug(string message);

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        void Info(string message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        void Warning(string message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        void Error(string message);

        /// <summary>
        /// Logs an error message with exception details.
        /// </summary>
        void Error(string message, Exception ex);

        /// <summary>
        /// Logs a fatal error message.
        /// </summary>
        void Fatal(string message);

        /// <summary>
        /// Logs a fatal error message with exception details.
        /// </summary>
        void Fatal(string message, Exception ex);
    }

    /// <summary>
    /// Logger implementation that writes to the MainBoard console.
    /// Provides backward compatibility with existing MainBoard.WriteLine calls.
    /// </summary>
    public class MainBoardLogger : ILogger
    {
        private static MainBoardLogger _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the singleton instance of the MainBoardLogger.
        /// </summary>
        public static MainBoardLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new MainBoardLogger();
                        }
                    }
                }
                return _instance;
            }
        }

        private MainBoardLogger() { }

        public void Log(LogLevel level, string message)
        {
            Color color = GetColorForLevel(level);
            string prefix = GetPrefixForLevel(level);
            MainBoard.WriteLine($"{prefix}{message}", color);
        }

        public void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        public void Warning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        public void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        public void Error(string message, Exception ex)
        {
            Log(LogLevel.Error, $"{message}: {ex.Message}");
        }

        public void Fatal(string message)
        {
            Log(LogLevel.Fatal, message);
        }

        public void Fatal(string message, Exception ex)
        {
            Log(LogLevel.Fatal, $"{message}: {ex.Message}");
        }

        private static Color GetColorForLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return Color.Gray;
                case LogLevel.Info:
                    return Color.Black;
                case LogLevel.Warning:
                    return Color.Orange;
                case LogLevel.Error:
                    return Color.Red;
                case LogLevel.Fatal:
                    return Color.DarkRed;
                default:
                    return Color.Black;
            }
        }

        private static string GetPrefixForLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return "[DEBUG] ";
                case LogLevel.Warning:
                    return "[WARN] ";
                case LogLevel.Error:
                    return "[ERROR] ";
                case LogLevel.Fatal:
                    return "[FATAL] ";
                default:
                    return "";
            }
        }
    }

    /// <summary>
    /// Static logger facade for easy access throughout the application.
    /// </summary>
    public static class Log
    {
        private static ILogger _logger = MainBoardLogger.Instance;

        /// <summary>
        /// Sets a custom logger implementation (useful for testing).
        /// </summary>
        public static void SetLogger(ILogger logger)
        {
            _logger = logger ?? MainBoardLogger.Instance;
        }

        public static void Debug(string message) => _logger.Debug(message);
        public static void Info(string message) => _logger.Info(message);
        public static void Warning(string message) => _logger.Warning(message);
        public static void Error(string message) => _logger.Error(message);
        public static void Error(string message, Exception ex) => _logger.Error(message, ex);
        public static void Fatal(string message) => _logger.Fatal(message);
        public static void Fatal(string message, Exception ex) => _logger.Fatal(message, ex);
    }
}
