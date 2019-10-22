using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using DirtBot.Services;
using DirtBot.Caching;

namespace DirtBot
{
    class DirtBot
    {
        public async Task StartAsync()
        {
            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();

                // Internal
                client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;
                Config config = services.GetRequiredService<Config>();

                // Cache
                services.GetRequiredService<Cache>();
                CacheThread cacheThread = services.GetRequiredService<CacheThread>();
                services.GetRequiredService<AutoCacher>();

                Thread thread = new Thread(CacheThread.InitiazeCacheThread);
                thread.Start(services);

                // Login
                await client.LoginAsync(TokenType.Bot, config.Token);
                await client.StartAsync();

                // Emojis
                Emojis emojis = services.GetRequiredService<Emojis>();
                emojis.AddEmoji("<:dirtdontpingme:634748801617231892>");

                // Initializing services
                services.GetRequiredService<CommandHandlingService>();
                services.GetRequiredService<Ping>();
                services.GetRequiredService<Scares>();
                services.GetRequiredService<FsInTheChat>();
                services.GetRequiredService<Goodbye>();
                services.GetRequiredService<Greetings>();
                services.GetRequiredService<DontPingMe>();

                // Making sure we won't fall off the loop and keep the bot online
                await Task.Delay(-1);
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Discord.Net stuff
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<HttpClient>()
                // Config and internal stuff
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<CacheThread>()
                .AddSingleton<AutoCacher>()
                .AddSingleton<Cache>()
                .AddSingleton<Config>()
                .AddSingleton<Emojis>()
                // Other services
                .AddSingleton<Ping>()
                .AddSingleton<Scares>()
                .AddSingleton<FsInTheChat>()
                .AddSingleton<Goodbye>()
                .AddSingleton<Greetings>()
                .AddSingleton<DontPingMe>()
                // Build
                .BuildServiceProvider();
        }
    }
}
