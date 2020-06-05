using DirtBot.Core;
using DSharpPlus.CommandsNext;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DirtBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string logFile = $"logs/{((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds()}.log";
            Directory.CreateDirectory("logs/");
            File.Create(logFile).Close();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                try
                {
                    File.AppendAllText(logFile, $"The application has thrown an unhandled exception: {e.ExceptionObject}\n");
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
            File.AppendAllText(logFile, restart + "\n");

            var config = Configuration.LoadConfiguration("config.yml");
            config.AddDefaultValue("token", "");
            config.AddDefaultValue("redis_url", "localhost");
            config.AddDefaultValue("prefix", "dirt ");
            config.Save();

            string token = config.GetValue("token").ToString();
            string redisUrl = config.GetValue("redis_url").ToString();
            string prefix = config.GetValue("prefix").ToString();

            // For copy paste reasons
            var bot = new global::DirtBot.Core.DirtBot(new DirtBotConfiguration()
            {
                CommandPrefix = prefix,
                Token = token,
                RedisUrl = redisUrl,
                LogLevel = DirtBot.Core.LogLevel.Info,
                LogFile = logFile,
            });

            foreach (var file in Directory.EnumerateFiles("modules/", "*.dll"))
            {
                var a = Assembly.LoadFrom(file);
                bot.AddCommands(a);
                bot.AddModules(a);
            }
            
            await bot.StartAsync();
            await Task.Delay(-1);
        }
    }
}