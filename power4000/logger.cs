using System;
using System.IO;
using System.Text;

namespace power4000
{
    public static class Logger
    {
        private static readonly string LogDirectory = @"C:\Users\hahj1\source\repos\power4000\power4000\bin\Debug\Log";
        private static readonly string LogFilePrefix = "log_";
        private static readonly long MaxLogFileSize = 2 * 1024 * 1024; // 2 MB

        public static void Log(string message)
        {
            string logFilePath = GetLogFilePath();
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}";

            File.AppendAllText(logFilePath, logEntry, Encoding.UTF8);
        }

        public static void LogData(string data, string type)
        {
            string logFilePath = GetLogFilePath();
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"{currentTime} [{type}] {data}{Environment.NewLine}";

            File.AppendAllText(logFilePath, logEntry, Encoding.UTF8);
        }

        private static string GetLogFilePath()
        {
            string logFilePath = $"{LogDirectory}{LogFilePrefix}{DateTime.Now:yyyyMMdd_HHmmss}.txt";

            if (Directory.Exists(LogDirectory))
            {
                var logFiles = Directory.GetFiles(LogDirectory, $"{LogFilePrefix}*.txt");
                if (logFiles.Length > 0)
                {
                    logFilePath = logFiles[logFiles.Length - 1];
                    FileInfo fileInfo = new FileInfo(logFilePath);
                    if (fileInfo.Length >= MaxLogFileSize)
                    {
                        logFilePath = $"{LogDirectory}{LogFilePrefix}{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(LogDirectory);
            }

            return logFilePath;
        }
    }
}
