using DSharpPlus;
using DSharpPlus.EventArgs;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DirtBot
{
    public class Logger
    {
        readonly string application;

        public Logger(string application)
        {
            this.application = application;
        }

        public static Logger GetLogger(object obj)
        {
            return new Logger(obj.GetType().FullName);
        }

        public void Info(string message) => Write(message, LogLevel.Info);
        public void Debug(string message) => Write(message, LogLevel.Debug);
        public void Warning(string message) => Write(message, LogLevel.Warning);
        public void Error(string message, Exception e) => Write(message + $" Exception: {e}", LogLevel.Error);
        public void Critical(string message) => Write(message, LogLevel.Critical);

        public void Write(string message, LogLevel level)
        {
            string[] lines = SplitLines(message);
            foreach (string line in lines)
            {
                DirtBot.Client.DebugLogger.LogMessage(level, application, line, DateTime.Now);
            }
        }

        static string[] SplitLines(string message)
        {
            return message.Split("\n");
        }

        #region File Logging
        internal static void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            WriteLogFileInternal(sender, e);
        }

        private static Task WriteLogFileInternal(object sender, DebugLogMessageEventArgs e)
        {
            for (int i = 0; i < 1001; i++)
            {
                try
                {
                    File.AppendAllText("log.txt", e.ToString() + "\n");
                    return Task.CompletedTask;
                }
                catch (Exception)
                {
                    Thread.Sleep(5);
                }
            }
            // Don't use the DSharpPlus logger here, it can accidetally make a loop and break stuff bad.
            Console.WriteLine($"Logger: Failed to write to log file!");
            return Task.CompletedTask;
        }
        #endregion
    }
}
