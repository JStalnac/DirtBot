using DirtBot.Database.FileManagement;
using SmartFormat;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DirtBot.Logging
{
    public static class Logger
    {
        static ManagedFile logFile;
        public static string FileName { get; private set; } = "log.txt";

        public static string LogMessageFormat { get; set; } = "[{0}]: {1}: {2}";

        static Logger()
        {
            if (!File.Exists(FileName))
                File.Create(FileName).Close();
            
            logFile = new ManagedFile(FileName);
        }

        public static void Log(string message, bool writeFile = false, Exception exception = null, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            StackFrame frame = new StackTrace().GetFrame(1);
            string methodString = GetMethodString(frame.GetMethod());

            LogInternal(source: methodString, message: message, writeFile: writeFile, exception: exception, foregroundColor: foregroundColor, backgroundColor: backgroundColor);
        }

        public static Task LogInternal(string source, string message, bool writeFile, Exception exception, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;

            string time = DateTime.Now.ToString();

            if (!(exception is null))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                if (Config.LogTraces)
                    Console.WriteLine($"{source}: {message} Exception:\n{exception}");
                else
                    Console.WriteLine($"{source}: {message} Exception:\n{exception.Message}");
            }
            else
                try
                {
                    Console.WriteLine(LogMessageFormat.FormatSmart(time, source, message.Trim()));
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(LogMessageFormat.FormatSmart(time, source, message));
                }

            // Remember to reset the color to default!
            Console.ResetColor();

            // Write log file
            if (exception is null)
                WriteLogFileInternal(source, message);
            else
                WriteLogFileInternal(source, message + $"Exception:\n{exception}");

            return Task.CompletedTask;
        }

        public static void WriteLogFile(string message)
        {
            StackFrame frame = new StackTrace().GetFrame(1);
            string methodString = GetMethodString(frame.GetMethod());

            WriteLogFileInternal(methodString, message);
        }

        private static void WriteLogFileInternal(string source, string message)
        {
            string time = DateTime.Now.ToString();
            logFile.Refresh();
            logFile.EnsureExists();

            if (!(message is null))
                logFile.AppendAllTextAsync($"[{time}] {source}: {message.Trim()}\n");
            else
                logFile.AppendAllTextAsync($"[{time}] {source}: {message}\n");
        }

        public static string GetMethodString(MethodBase method)
        {
            string name;
            var a = method.GetCustomAttribute(typeof(LoggerNameAttribute)) as LoggerNameAttribute;
            name = a is null ? method.Name : a.Name;

            string methodString = $"{method.DeclaringType}.{name}(";
            ParameterInfo[] parameters = method.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                methodString += $"{parameters[i].ParameterType} {parameters[i].Name}";

                if (i != parameters.Length - 1)
                    methodString += ", ";
            }
            // Close the "method"
            methodString += ")";
            return methodString;
        }
    }
}
