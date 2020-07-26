using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LCDWidget
{
    public static class Logger
    {
        private static string logDir = "log/";
        private static string debugLogDir = logDir + "debug/";
        private static string filePath;
        private static object fileLock = new object();

        public static bool DoDebugLog { get; set; }

        public enum LogType
        {
            Log,
            Debug
        }

        public static void InitLogger(bool doDebugLog = false)
        {
            DoDebugLog = doDebugLog;

            Directory.CreateDirectory(logDir);

            DateTime currentTime = DateTime.Now;
            filePath = $"{currentTime.Year}-{currentTime.Month}-{currentTime.Day}_{currentTime.Hour}-{currentTime.Minute}-{currentTime.Second}.log";

            if (doDebugLog)
            {
                Directory.CreateDirectory(debugLogDir);
            }
        }

        public static void Log(string message, Type origin = null, LogType type = LogType.Log)
        {
            if (type == LogType.Debug && DoDebugLog == false) return;

            StringBuilder logMessage = new StringBuilder();
            DateTime currentTime = DateTime.Now;
            logMessage.Append($"[{currentTime.Year}-{currentTime.Month}-{currentTime.Day} {currentTime.Hour:00}:{currentTime.Minute:00}:{currentTime.Second:00}] ");
            if (origin != null) logMessage.Append($"{origin.Name} - ");
            logMessage.Append(message);

            string path = type switch
            {
                LogType.Log => logDir + filePath,
                LogType.Debug => debugLogDir + filePath,
                _ => logDir + filePath
            };

            lock (fileLock)
            {
                using StreamWriter writer = new StreamWriter(path, true);
                writer.WriteLine(logMessage.ToString());
            }
        }
    }
}
