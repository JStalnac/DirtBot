using System;
using System.Collections.Generic;
using System.IO;
using DirtBot.Translation;

namespace DirtBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");
            string logFile = $"logs/{DateTimeOffset.Now.ToUnixTimeSeconds()}.log";

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var log = Logger.GetLogger("Exception Logger");
                log.Critical($"The application has thrown an unhandled exception.\nIf you think this is an error please report it on Github with the stack trace :) https://github.com/JStalnac/DirtBot/issues\nTerminating: {e.IsTerminating}\nCleaning up");

                try
                {
                    // Write exception
                    File.AppendAllText(logFile, e.ExceptionObject.ToString());
                    // Clean up stuff safely
                    CleanUp();
                }
                catch { }
            };
            // Console cleaning
            Console.CancelKeyPress += (sender, e) =>
            {
                var log = Logger.GetLogger("Main");
                log.Important("Cancel key press received.\nCleaning up...");
                CleanUp();
                log.Important("Done!\nExit 0");
                Environment.Exit(0);
            };

            string PadCenter(string s, int width, char c)
            {
                if (s == null || width <= s.Length) return s;

                int padding = width - s.Length;
                return s.PadLeft(s.Length + padding / 2, c).PadRight(width, c);
            }

            // Enable file output
            Logger.SetLogFile(logFile);

            string restart = " -[ RESTART ]- ";
            restart = PadCenter(restart, 90, '=');
            File.AppendAllText(logFile, restart + "\n");

            var log = Logger.GetLogger("Main");
            log.Important("Starting! Hello World!");

            new Dirtbot().StartAsync().Wait();
        }

        static void CleanUp()
        {
            Dirtbot.Client?.LogoutAsync();
            Dirtbot.Redis?.Close(true);
        }
    }
}