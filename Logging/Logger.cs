using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SmartFormat;

namespace DirtBot.Logging
{

    internal class Logger
    {
        string source;
        LogSeverity logSeverity;

        public Logger(string source, LogSeverity severity)
        {
            this.source = source;
            logSeverity = severity;
        }

        public string Source
        {
            get { return source; }
            set { source = value; }
        }

        public async Task LogAsync(LogSeverity severity, string message)
        {
            if (severity <= logSeverity)
                Console.WriteLine("[ {2} | {0} {1} ]: {3}".FormatSmart(DateTime.Now.ToString("dd'/'MM'/'yy H:mm:ss"), source, severity, message));
        }

        public Task CriticalAsync(string message)
            => LogAsync(LogSeverity.Critical, message);

        public Task ErrorAsync(string message)
            => LogAsync(LogSeverity.Error, message);

        public Task WarningAsync(string message)
            => LogAsync(LogSeverity.Warning, message);

        public Task InfoAsync(string message)
            => LogAsync(LogSeverity.Info, message);

        public Task VerboseAsync(string message)
            => LogAsync(LogSeverity.Verbose, message);

        public Task DebugAsync(string message)
            => LogAsync(LogSeverity.Debug, message);

        public static Logger GetLogger(object logger)
        {
            return new Logger(logger.GetType().Name, LogSeverity.Debug);
        }

        public static Logger GetLogger(string source)
        {
            return new Logger(source, LogSeverity.Debug);
        }

        public static Logger GetLogger(object logger, LogSeverity severity)
        {
            return new Logger(logger.GetType().Name, severity);
        }

        public static Logger GetLogger(string source, LogSeverity severity)
        {
            return new Logger(source, severity);
        }
    }
}
