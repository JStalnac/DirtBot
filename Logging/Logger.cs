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

using Pastel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DirtBot.Logging
{
    /// <summary>
    /// Log level that the logger uses.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug log level.
        /// </summary>
        Debug,

        /// <summary>
        /// Information log level.
        /// </summary>
        Info,

        /// <summary>
        /// Warning log level.
        /// </summary>
        Warning,

        /// <summary>
        /// Error log level.
        /// </summary>
        Error,

        /// <summary>
        /// Important log level.
        /// </summary>
        Important,

        /// <summary>
        /// Critical log level.
        /// </summary>
        Critical
    }

    public sealed class Logger
    {
        /// <summary>
        /// Gets or sets the minumum log level for the loggers.
        /// </summary>
        public static LogLevel MinimumLogLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// Gets or sets the log file path that logs will be written into.
        /// </summary>
        public static string LogFile
        {
            get => logFile;
            set
            {
                if (value == null)
                {
                    logFile = null;
                    return;
                }

                if (String.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value));

                try
                {
                    // This has flaws
                    Path.GetFullPath(value);
                }
                catch
                {
                    throw new ArgumentException("Invalid path", nameof(value));
                }

                logFile = value;
            }
        }
        private static string logFile;

        /// <summary>
        /// Gets or sets the datetime format the loggers use.
        /// </summary>
        public static string DateTimeFormat
        {
            get => datetimeFormat;
            set
            {
                if (String.IsNullOrEmpty(value) || String.IsNullOrWhiteSpace(value))
                    throw new ArgumentException(nameof(value));

                try
                {
                    DateTime.Now.ToString(value);
                }
                catch (FormatException)
                {
                    throw new FormatException("Invalid datetime format");
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new FormatException("Invalid datetime");
                }

                datetimeFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the full namespace and name of a type should be used or just the name when naming loggers.
        /// </summary>
        public static bool UseTypeFullName { get; set; }

        // I'm not American
        private static string datetimeFormat = "dd/MM/yyyy HH:mm:ssZzzz";

        private readonly string name;

        /// <summary>
        /// Initializes a new instance of the Logger class.
        /// </summary>
        /// <param name="name">The name the logger will log messages with.</param>
        public Logger(string name)
        {
            // Sanitize the input
            string cleanName = Regex.Replace(name, "[\x00-\x1F\x7F]", "");
            if (String.IsNullOrEmpty(cleanName) || String.IsNullOrWhiteSpace(cleanName))
                throw new ArgumentNullException(nameof(name));
            this.name = cleanName;
        }

        /// <summary>
        /// Sets the log file used. If no file is set messages won't be logged to a file.
        /// </summary>
        /// <param name="path"></param>
        [Obsolete]
        public static void SetLogFile(string path) => LogFile = path;

        /// <summary>
        /// Sets the minimum log level. The default level is <see cref="LogLevel.Info"/>
        /// </summary>
        /// <param name="logLevel"></param>
        [Obsolete]
        public static void SetLogLevel(LogLevel logLevel) => MinimumLogLevel = logLevel;

        /// <summary>
        /// Sets the datetime format used.
        /// </summary>
        /// <param name="format"></param>
        [Obsolete]
        public static void SetDateTimeFormat(string format) => DateTimeFormat = format;

        /// <summary>
        /// Gets a new logger for the specified type.
        /// </summary>
        /// <returns></returns>
        public static Logger GetLogger<T>(T type) => GetLogger<T>();

        /// <summary>
        /// Gets a new logger for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Logger GetLogger<T>() => UseTypeFullName ? new Logger(typeof(T).FullName) : new Logger(typeof(T).Name);

        /// <summary>
        /// Gets a new logger with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Logger GetLogger(string name) => new Logger(name);

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        /// <summary>
        /// Creates a console window using AllocConsole.
        /// Works only on Windows
        /// </summary>
        public static void CreateConsole()
        {
            // Only try calling AllocConsole on Windows
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                AllocConsole();
        }

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Debug"/> log level.
        /// </summary>
        /// <param name="message">Log message</param>
        public void Debug(string message) => Write(message, LogLevel.Debug);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Debug"/> log level including an exception.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name ="e">Exception</param>
        public void Debug(string message, Exception e) => Write(message, LogLevel.Debug, e);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Debug"/> log level using the provided object's <see cref="System.Object.ToString"/> method.
        /// </summary>
        /// <param name="obj">Object</param>
        public void Debug(object obj) => Write(obj, LogLevel.Debug);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Info"/> log level.
        /// </summary>
        /// <param name="message">Log message</param>
        public void Info(string message) => Write(message, LogLevel.Info);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Info"/> log level including an exception.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name ="e">Exception</param>
        public void Info(string message, Exception e) => Write(message, LogLevel.Info, e);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Info"/> log level using the provided object's <see cref="System.Object.ToString"/> method.
        /// </summary>
        /// <param name="obj">Object</param>
        public void Info(object obj) => Write(obj, LogLevel.Info);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Warning"/> log level.
        /// </summary>
        /// <param name="message">Log message</param>
        public void Warning(string message) => Write(message, LogLevel.Warning);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Warning"/> log level including an exception.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name ="e">Exception</param>
        public void Warning(string message, Exception e) => Write(message, LogLevel.Warning, e);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Warning"/> log level using the provided object's <see cref="System.Object.ToString"/> method.
        /// </summary>
        /// <param name="obj">Object</param>
        public void Warning(object obj) => Write(obj, LogLevel.Warning);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Error"/> log level.
        /// </summary>
        /// <param name="message">Log message</param>
        public void Error(string message) => Write(message, LogLevel.Error);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Error"/> log level including an exception.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name ="e">Exception</param>
        public void Error(string message, Exception e) => Write(message, LogLevel.Error, e);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Error"/> log level using the provided object's <see cref="System.Object.ToString"/> method.
        /// </summary>
        /// <param name="obj">Object</param>
        public void Error(object obj) => Write(obj, LogLevel.Error);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Important"/> log level.
        /// </summary>
        /// <param name="message">Log message</param>
        public void Important(string message) => Write(message, LogLevel.Important);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Important"/> log level including an exception.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name ="e">Exception</param>
        public void Important(string message, Exception e) => Write(message, LogLevel.Important, e);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Important"/> log level using the provided object's <see cref="System.Object.ToString"/> method.
        /// </summary>
        /// <param name="obj">Object</param>
        public void Important(object obj) => Write(obj, LogLevel.Important);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Critical"/> log level.
        /// </summary>
        /// <param name="message">Log message</param>
        public void Critical(string message) => Write(message, LogLevel.Critical);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Critical"/> log level including an exception.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name ="e">Exception</param>
        public void Critical(string message, Exception e) => Write(message, LogLevel.Critical, e);

        /// <summary>
        /// Writes a log message on <see cref="LogLevel.Critical"/> log level using the provided object's <see cref="System.Object.ToString"/> method.
        /// </summary>
        /// <param name="obj">Object</param>
        public void Critical(object obj) => Write(obj, LogLevel.Critical);

        private static Color GetColor(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Debug => Color.FromArgb(0x0f960d),
                LogLevel.Info => Color.FromArgb(0xeaeaea),
                LogLevel.Warning => Color.FromArgb(0xc6ad0b),
                LogLevel.Error => Color.FromArgb(0xd30c0c),
                LogLevel.Important => Color.FromArgb(0x02fcf4),
                LogLevel.Critical => Color.FromArgb(0xff0000),
                _ => Color.LightGray
            };
        }

        /// <summary>
        /// Writes a log message using an object's <see cref="System.Object.ToString"/> method.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="logLevel">Log level</param>
        public void Write(object obj, LogLevel logLevel) => Write(obj?.ToString(), logLevel, null);

        /// <summary>
        /// Writes a log message using the provided log level including an exception.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="logLevel">Log level</param>
        /// <param name="exception">Exception</param>
        public void Write(string message, LogLevel logLevel, Exception exception = null)
        {
            if (MinimumLogLevel > logLevel)
                return;
            Write(message, GetColor(logLevel), exception: exception, logLevel: logLevel);
        }

        /// <summary>
        /// Writes a log message in a custom color using an object's <see cref="System.Object.ToString"/> method.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="fore">Foreground color</param>
        /// <param name="back">Background color</param>
        public void Write(object obj, Color fore, Color? back = null) => Write(obj?.ToString(), fore, back);

        private static object writeLock = new object();

        /// <summary>
        /// Writes a log message using a custom color including an exception.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="fore">Foreground color</param>
        /// <param name="back">Background color</param>
        /// <param name="exception">Exception</param>
        /// <param name="logLevel">Log level. Don't use this manually.</param>
        public void Write(string message, Color fore, Color? back = null, Exception exception = null,
            LogLevel logLevel = LogLevel.Info)
        {
            // This allows us to write safely multiple lines
            // and to a file.
            lock (writeLock)
            {
                if (message == null || String.IsNullOrEmpty(message.Trim()))
                {
                    if (exception == null)
                        message = "null"; // No message, no exception
                }

                var lines = new List<string>();

                // Append message
                if (message != null)
                {
                    message = message.Trim();
                    lines.AddRange(message.Split('\n'));
                }

                // Append exception
                if (exception != null)
                    lines.AddRange(exception.ToString().Split('\n'));

                string prefix =
                    $"[{DateTime.Now.ToString(datetimeFormat, CultureInfo.InvariantCulture)}] [{name}] [{logLevel}]";

                // Begin write to file
                Task fileWrite = null;
                try
                {
                    if (!String.IsNullOrEmpty(logFile))
                    {
                        // For safety reasons, the variable might get modified before it's written to the log file
                        string p = prefix;
                        fileWrite = File.AppendAllLinesAsync(logFile, lines.Select(x => $"{p} {x?.TrimEnd()}"));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write to log file: {ex.Message}");
                }

                // Write to console

                // Coloring
                prefix = prefix.Pastel(fore);
                if (back.HasValue)
                    prefix = prefix.PastelBg(back.Value);

                foreach (string line in lines)
                    // Prevent ANSI escape sequences in the message
                    Console.WriteLine($"{prefix} {line.Replace("\x1b", "")}");

                // End write to file
                fileWrite?.Wait();
            }
        }
    }
}
