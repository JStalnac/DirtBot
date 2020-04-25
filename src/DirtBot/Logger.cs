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
        LogLevel level;
        const string datetimeFormat = "yyyy-MM-dd HH:mm:ss zzz";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="application">The name the logger will log messages with.</param>
        /// <param name="level">The minimum <see cref="DSharpPlus.LogLevel"/> that messages will be logged.</param>
        public Logger(string application, LogLevel level = LogLevel.Info)
        {
            this.application = application;
            this.level = level;
        }
        
        /// <summary>
        /// <see cref="Logger.Logger(string, LogLevel)"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static Logger GetLogger(object obj, LogLevel level = LogLevel.Info)
        {
            return new Logger(obj.GetType().FullName, level);
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
            if (this.level >= level)
            {
                DebugLogger_LogMessageReceived(level, application, message, exception, DateTime.Now);
            }
        }

        internal static void DebugLogger_LogMessageReceived(LogLevel level, string application, string message, Exception exception, DateTime timestamp)
        {
            if (message == null || String.IsNullOrEmpty(message.Trim()))
                if (exception == null)
                    message = "null";

            var lines = new List<string>();

            if (message != null)
            {
                message = message.Trim();
                lines.AddRange(message.Split("\n"));
            }

            if (exception != null)
                lines.AddRange(exception.ToString().Split("\n"));

            string prefix = $"[{timestamp.ToString(datetimeFormat)}] [{application}] [{level}]";

            WriteLogMessageInternal(lines, level, prefix);
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
