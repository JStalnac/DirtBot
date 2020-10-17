/*
 * Copyright 2020 JStalnac
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Microsoft.Extensions.Logging;
using System;

namespace DirtBot.Logging
{
    public static class LoggerBuilderExtensions
    {
        public static ILoggingBuilder AddJStalnacLogging(this ILoggingBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.AddProvider(new JStalnacLoggerProvider());
            builder.AddFilter<JStalnacLoggerProvider>(null, Microsoft.Extensions.Logging.LogLevel.Trace);

            return builder;
        }
    }

    [ProviderAlias("JStalnac.Common.Logging")]
    internal class JStalnacLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
            => new JStalnacLogger(categoryName);

        public void Dispose() { }
    }

    internal class JStalnacLogger : ILogger
    {
        private readonly Logger logger;

        public JStalnacLogger(string name)
        {
            logger = Logger.GetLogger(name);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return GetLevel(logLevel) >= Logger.MinimumLogLevel;
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;
            logger.Write(state?.ToString(), GetLevel(logLevel), exception);
        }

        private LogLevel GetLevel(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return logLevel switch
            {
                Microsoft.Extensions.Logging.LogLevel.Trace => LogLevel.Debug,
                Microsoft.Extensions.Logging.LogLevel.Debug => LogLevel.Debug,
                Microsoft.Extensions.Logging.LogLevel.Information => LogLevel.Info,
                Microsoft.Extensions.Logging.LogLevel.Warning => LogLevel.Warning,
                Microsoft.Extensions.Logging.LogLevel.Error => LogLevel.Error,
                Microsoft.Extensions.Logging.LogLevel.Critical => LogLevel.Critical,
                Microsoft.Extensions.Logging.LogLevel.None => LogLevel.Info,
                _ => LogLevel.Info,
            };
        }
    }
}
