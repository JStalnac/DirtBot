using DirtBot.Logging;
using System;
using System.IO;

namespace DirtBot
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                Logger.WriteLogFile($"The application has thrown an unhandled exception.\n{ex}\n\nIf you think this is an error please report it on Github with the stack trace :) https://github.com/JStalnac/DirtBot/issues \nTerminating: {e.IsTerminating}");
                try
                {
                    // Clean up stuff safely
                    DirtBot.Client.LogoutAsync();
                }
                catch (Exception) { }
            };

            string PadCenter(string s, int width, char c)
            {
                if (s == null || width <= s.Length) return s;

                int padding = width - s.Length;
                return s.PadLeft(s.Length + padding / 2, c).PadRight(width, c);
            }

            string restart = " -[ RESTART ]- ";
            restart = PadCenter(restart, 90, '=');
            File.AppendAllText(Logger.FileName, restart + "\n");

            Logger.Log("Starting! Hello World!");
            new DirtBot().StartAsync().GetAwaiter().GetResult();
        }
    }
}
