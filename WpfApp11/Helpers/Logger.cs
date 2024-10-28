using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp11.Helpers
{
 
    public static class Logger
    {
        public static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "device_logs"+ $"{DateTime.Now:yyyy-MM-dd}"+".txt");
        public static readonly string LogErrorFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_logs.txt");

        public static void Log(string deviceName, string deviceType, string action, string result)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - 아이피: {deviceName} ({deviceType}) - 포트: {action} - 내용: {result}";
            
            try
            {
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        public static void Log2(string result)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - 내용: {result}";

            try
            {
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        public static void LogError(string result)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - 내용: {result}";

            try
            {
                File.AppendAllText(LogErrorFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
    }
}
