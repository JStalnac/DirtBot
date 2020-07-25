using System;
using System.IO;

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
            };

            string PadCenter(string s, int width, char c)
            {
                if (s == null || width <= s.Length) return s;

                int padding = width - s.Length;
                return s.PadLeft(s.Length + padding / 2, c).PadRight(width, c);
            }

            Logger.SetLogFile(logFile);
            var restart = " -[ RESTART ]- ";
            restart = PadCenter(restart, 90, '=');
            File.AppendAllText(logFile, restart + "\n");

            Logger.GetLogger("Main").Important("Starting! Hello World!");
            new Dirtbot().StartAsync().Wait();
        }
        
        static void CleanUp()
        {
            Dirtbot.Client?.LogoutAsync();
            Dirtbot.Redis?.Close(true);
        }
    }
}