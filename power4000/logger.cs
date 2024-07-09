using System;
using System.IO;
using System.Text;

namespace power4000
{
    public static class Logger
    {
        private static readonly string LogDirectory = @"D:\ConWell\Conwell\power4000\power4000\bin\Debug\Log";
        private static readonly string LogFilePrefix = "log_";
        private static readonly long MaxLogFileSize = 2 * 1024 * 1024; // 2 MB
        private static readonly TimeSpan LogFileDuration = TimeSpan.FromHours(4); // 4 hours

        public static void Log(string message)
        {
            string logFilePath = GetLogFilePath();
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}";

            File.AppendAllText(logFilePath, logEntry, Encoding.UTF8);
        }

        public static void LogData(string data, string type)
        {
            string logFilePath = GetLogFilePath();
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss: ");
            string logEntry = $"{currentTime} [{type}] {data}{Environment.NewLine}";

            File.AppendAllText(logFilePath, logEntry, Encoding.UTF8);
        }

        private static string GetLogFilePath()
        {
            string logFilePath = $"{LogDirectory}\\{LogFilePrefix}{DateTime.Now:yyyyMMdd_HHmmss}.txt";

            if (Directory.Exists(LogDirectory))
            {
                var logFiles = Directory.GetFiles(LogDirectory, $"{LogFilePrefix}*.txt");
                if (logFiles.Length > 0)
                {
                    Array.Sort(logFiles);
                    Array.Reverse(logFiles); // Newest file first

                    foreach (var file in logFiles)
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        if (fileInfo.CreationTimeUtc <= DateTime.UtcNow - LogFileDuration ||
                            fileInfo.Length >= MaxLogFileSize)
                        {
                            continue;
                        }
                        logFilePath = file;
                        break;
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(LogDirectory);
            }

            // If logFilePath is still the default path, create a new file
            if (logFilePath == $"{LogDirectory}\\{LogFilePrefix}{DateTime.Now:yyyyMMdd_HHmmss}.txt")
            {
                logFilePath = $"{LogDirectory}\\{LogFilePrefix}{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            }

            return logFilePath;
        }
    }
}
