using DSharpPlus;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DirtBot
{
    public class Logger
    {
        readonly string application;
        const string datetimeFormat = "yyyy-MM-dd HH:mm:ss zzz";

        public Logger(string application)
        {
            this.application = application;
        }

        public static Logger GetLogger(object obj)
        {
            return new Logger(obj.GetType().FullName);
        }

        public void Info(string message) => Write(message, LogLevel.Info);
        public void Info(string message, Exception e) => Write(message, LogLevel.Info, e);
        public void Debug(string message) => Write(message, LogLevel.Debug);
        public void Debug(string message, Exception e) => Write(message, LogLevel.Debug, e);
        public void Warning(string message) => Write(message, LogLevel.Warning);
        public void Warning(string message, Exception e) => Write(message, LogLevel.Warning, e);
        public void Error(string message) => Write(message, LogLevel.Error);
        public void Error(string message, Exception e) => Write(message, LogLevel.Error, e);
        public void Critical(string message) => Write(message, LogLevel.Critical);
        public void Critical(string message, Exception e) => Write(message, LogLevel.Critical, e);

        public void Write(string message, LogLevel level, Exception exception = null)
        {
            if (message == null || String.IsNullOrEmpty(message.Trim()))
                if (exception is null)
                    message = "<None>";

            var lines = new List<string>();

            if (message != null)
                lines.AddRange(message.Trim().Split("\n"));
            
            if (exception != null)
                lines.AddRange(exception.ToString().Split("\n"));
            
            foreach (string line in lines)
                DirtBot.Client.DebugLogger.LogMessage(level, application, line, DateTime.Now);
        }

        #region File Logging
        internal static void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            WriteLogFileInternal(sender, e);
        }

        private static Task WriteLogFileInternal(object sender, DebugLogMessageEventArgs e)
        {
            string prefix = $"[{e.Timestamp.ToString(datetimeFormat)}] [{e.Application}] [{e.Level}] ";
            var lines = new List<string>(e.Message.Trim().Split("\n"));
            
            for (int i = 0; i < lines.Count; i++)
            {
                if (String.IsNullOrEmpty(lines[i]))
                {
                    lines[i] = "<None>";
                }
                lines[i] = prefix + lines[i];
            }

            for (int i = 0; i < 1001; i++)
            {
                try
                {
                    File.AppendAllLines("log.txt", lines);
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
