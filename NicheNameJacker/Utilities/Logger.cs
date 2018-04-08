using System;
using System.Text;

namespace NicheNameJacker.Utilities
{
    public static class Logger
    {
        private static readonly StringBuilder _log = new StringBuilder();

        public static string GetLog()
        {
            return _log.ToString();
        }

        public static void LogStatus(string status) => _log.AppendLine($"Status: {DateTime.Now} {status}");
        public static void LogStatuses(params string[] status)
        {
            for (int i = 0; i < status.Length; i++)
            {
                LogStatus(status[i]);
            }
        }

        public static void LogError(string error) => _log.AppendLine($"Error: {DateTime.Now} {error}");
    }
}
