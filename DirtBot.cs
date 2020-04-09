using DirtBot.Caching;
using DirtBot.Database;
using DirtBot.Logging;
using DirtBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace DirtBot
{
    class DirtBot
    {
        public static DiscordSocketClient Client { get; private set; }

        internal async Task StartAsync()
        {
            using (var services = ConfigureServices())
            {
                Client = services.GetRequiredService<DiscordSocketClient>();

                // Internal
                Client.Log += LogAsync;
                Client.Ready += async () =>
                {
                    Client.SetGameAsync("Being a good dirt blob");
                };

                services.GetRequiredService<CommandService>().Log += LogAsync;

                // Cache
                services.GetRequiredService<Cache>();
                services.GetRequiredService<Cacher>();

                //Thread thread = new Thread(Cacher.InitiazeCacheThread);
                //thread.Start();

                // Database connection check
                try
                {
                    DatabaseUtils.OpenConnection().Close();
                    Logger.Log("Succesfully connected to MySQL database!", true, foregroundColor: ConsoleColor.Cyan);
                }
                catch (MySqlException e)
                {
                    Logger.Log($"Database connection failed: {e.Message}", true, foregroundColor: ConsoleColor.Red);
                    Environment.Exit(-1);
                }
                catch (Exception e)
                {
                    Logger.Log($"Database connection failed: {e}", true, foregroundColor: ConsoleColor.Red);
                    Environment.Exit(-1);
                }

                // Login
                await Client.LoginAsync(TokenType.Bot, Config.Token);
                await Client.StartAsync();

                // Emojis
                Emojis emojis = services.GetRequiredService<Emojis>();

                // Initializing services
                services.GetRequiredService<CommandHandlingService>();
                services.GetRequiredService<Ping>();

                // Making sure we won't fall off the loop and keep the bot online
                await Task.Delay(-1);
            }
        }

        private Task LogAsync(LogMessage log)
        {
            StackFrame frame = new StackTrace().GetFrame(1);
            string source = "Discord Message: " + Logger.GetMethodString(frame.GetMethod());
            Logger.LogInternal(source: source, message: log.Message, writeFile: true, exception: log.Exception,
                foregroundColor: ConsoleColor.White, backgroundColor: ConsoleColor.Black);
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            // Socket configuration
            DiscordSocketConfig config = new DiscordSocketConfig()
            {
                ExclusiveBulkDelete = false,
            };

            return new ServiceCollection()
                // Discord.Net stuff
                .AddSingleton(new DiscordSocketClient(config))
                .AddSingleton<CommandService>()
                .AddSingleton<HttpClient>()
                // Config and internal stuff
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<Cacher>()
                .AddSingleton<Cache>()
                .AddSingleton<Emojis>()
                // Other services
                .AddSingleton<Ping>()
                // Build
                .BuildServiceProvider();
        }
    }
}
