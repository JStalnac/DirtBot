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
            DirtBot.Client.DebugLogger.LogMessage(level, application, message, DateTime.Now, exception);
        }

        internal static void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            string message = e.Message;
            if (message == null || String.IsNullOrEmpty(e.Message.Trim()))
                if (e.Exception == null)
                    message = "null";

            message = message.Trim();
            var lines = new List<string>();

            if (message != null)
                lines.AddRange(message.Split("\n"));

            if (e.Exception != null)
                lines.AddRange(e.Exception.ToString().Split("\n"));

            string prefix = $"[{e.Timestamp.ToString(datetimeFormat)}] [{e.Application}] [{e.Level}]";

            WriteLogMessageInternal(lines, e.Level, prefix);
            WriteLogFileInternal(lines, prefix);
        }

        private static Task WriteLogMessageInternal(List<string> lines, LogLevel level, string prefix)
        {
            ConsoleColor fore = Console.ForegroundColor;
            ConsoleColor back = Console.BackgroundColor;

            switch (level)
            {
                case LogLevel.Debug:
                    fore = ConsoleColor.DarkGreen;
                    break;

                case LogLevel.Info:
                    fore = ConsoleColor.White;
                    break;

                case LogLevel.Warning:
                    fore = ConsoleColor.DarkYellow;
                    break;

                case LogLevel.Error:
                    fore = ConsoleColor.DarkRed;
                    break;

                case LogLevel.Critical:
                    back = ConsoleColor.DarkRed;
                    fore = ConsoleColor.Black;
                    break;
            }

            foreach (string line in lines)
            {
                Console.ForegroundColor = fore;
                Console.BackgroundColor = back;
                Console.Write(prefix);
                Console.ResetColor();
                Console.WriteLine(" " + line);
            }
            return Task.CompletedTask;
        }

        private static Task WriteLogFileInternal(List<string> lines, string prefix)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i] = $"{prefix} {lines[i].TrimEnd()}";
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
    }
}
