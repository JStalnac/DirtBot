using DirtBot.Core;
using System;
using System.IO;

namespace DirtBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new Core.DirtBot();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                try
                {
                    File.AppendAllText("log.txt", $"The application has thrown an unhandled exception: {e.ExceptionObject}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write to log file. {ex}");
                }
            };

            string PadCenter(string s, int width, char c)
            {
                if (s == null || width <= s.Length) return s;

                int padding = width - s.Length;
                return s.PadLeft(s.Length + padding / 2, c).PadRight(width, c);
            }

            string restart = " -[ RESTART ]- ";
            restart = PadCenter(restart, 90, '=');
            File.AppendAllText("log.txt", restart + "\n");

            var config = Configuration.LoadConfiguration("config.yml");
            config.AddDefaultValue("token", "");
            config.AddDefaultValue("redis_url", "localhost");
            config.AddDefaultValue("prefix", "dirt ");
            config.Save();

            string token = config.GetValue("token").ToString();
            string redisUrl = config.GetValue("redis_url").ToString();
            string prefix = config.GetValue("prefix").ToString();

            bot.StartAsync(token, redisUrl, LogLevel.Debug, commandPrefix: prefix).Wait();
        }
    }
}