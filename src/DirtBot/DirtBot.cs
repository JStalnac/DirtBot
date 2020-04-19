using DirtBot.Database.FileManagement;
using DirtBot.Logging;
using DirtBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using StackExchange.Redis;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DirtBot
{
    public class DirtBot
    {
        public static DiscordSocketClient Client { get; private set; }

        public async Task StartAsync()
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

                // Loading guilds
                if (!Directory.Exists("guilds/")) Directory.CreateDirectory("guilds");
                FileManager.RegisterDirectory("Guilds", FileManager.LoadDirectory("guilds/"));

                services.GetRequiredService<ConnectionMultiplexer>();

                // Login
                try
                {
                    Logger.Log("Logging onto Discord", true, fore: ConsoleColor.White);
                    await Client.LoginAsync(TokenType.Bot, Config.Token);
                }
                catch (Discord.Net.HttpException)
                {
                    // Token is invalid
                    Logger.Log("Invalid token. Terminating", true, fore: ConsoleColor.Red);
                    Environment.Exit(-1);
                }
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

        [LoggerName("Discord")]
        private Task LogAsync(LogMessage log)
        {
            Logger.Log(log.Message, true, log.Exception, fore: ConsoleColor.White);
            return Task.CompletedTask;
        }

        [LoggerName("DirtBot Service Manager")]
        private ServiceProvider ConfigureServices()
        {
            CommandServiceConfig commandConfig = new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async
            };

            return new ServiceCollection()
                // Discord.Net stuff
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(new CommandService(commandConfig))
                .AddSingleton<HttpClient>()
                // Config and internal stuff
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<Emojis>()
                .AddSingleton(new Func<ConnectionMultiplexer>(() =>
                {
                    // Loading Redis in here
                    Logger.Log("Logging into Redis...", true, fore: ConsoleColor.White);
                    ConnectionMultiplexer redis = null;
                    try
                    {
                        redis = ConnectionMultiplexer.Connect(Config.RedisUrl);
                    }
                    catch (RedisConnectionException e)
                    {
                        Logger.Log("Failed to login to Redis. Check the redis address and the server and try again.", true, exception: e, fore: ConsoleColor.Red);
                        Logger.WriteLogFile("Terminating");
                        Environment.Exit(-1);
                    }
                    Logger.Log("Succesfully connected to Redis.", true, fore: ConsoleColor.White);
                    return redis;
                }).Invoke())
                // Other services
                .AddSingleton<Ping>()
                // Build
                .BuildServiceProvider();
        }
    }
}
