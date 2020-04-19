using DirtBot.Database.FileManagement;
using DirtBot.Logging;
using DirtBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
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

                // Login
                try
                {
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
                // Other services
                .AddSingleton<Ping>()
                // Build
                .BuildServiceProvider();
        }
    }
}
