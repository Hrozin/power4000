using System;
using System.IO;
using System.IO.Pipes;
using System.Windows.Forms;

namespace power4000
{
    public static class Logger
    {
        private static readonly object lockObj = new object();
        private static string logFilePath = "logfile.txt";

        public static void Log(string message)
        {
            lock (lockObj)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(logFilePath, true))
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Logging exception: " + ex.Message);
                }
            }
            
        }
    }
}
