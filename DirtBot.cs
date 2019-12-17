﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using DirtBot.Services;
using DirtBot.Caching;
using MySql.Data.MySqlClient;

namespace DirtBot
{
    class DirtBot
    {
        internal async Task StartAsync()
        {
            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();

                // Internal
                client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                // Cache
                services.GetRequiredService<Cache>();
                CacheThread cacheThread = services.GetRequiredService<CacheThread>();
                services.GetRequiredService<AutoCacher>();

                Thread thread = new Thread(CacheThread.InitiazeCacheThread);
                thread.Start(services);

                // Database connection check
                try
                {
                    services.GetRequiredService<MySqlConnection>().Open();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Database connection failed: {e.Message}");
                    Environment.Exit(-1);
                }

                // Login
                await client.LoginAsync(TokenType.Bot, Config.Token);
                await client.StartAsync();

                // Emojis
                Emojis emojis = services.GetRequiredService<Emojis>();

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
                .AddSingleton(new MySqlConnection(
                    $"Server={Config.DatabaseAddress};Database={Config.DatabaseName};Uid={Config.DatabaseUsername};Pwd={Config.DatabasePassword};")) // Creating the MySql connection here
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
