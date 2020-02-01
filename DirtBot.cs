﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Dash.CMD;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using DirtBot.Services;
using DirtBot.Caching;
using DirtBot.DataBase;
using DirtBot.DataBase.FileManagement;
using System.Collections.Generic;

namespace DirtBot
{
    class DirtBot
    {
        public static ServiceProvider Services { get; private set; }

        public async Task StartAsync()
        {
            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();
                Services = services;

                // Internal
                client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                // Load data folders
                if (!Directory.Exists("guilds/")) Directory.CreateDirectory("guilds/");
                FileManager.RegisterDirectory("Guilds", FileManager.LoadDirectory("guilds/"));

                if (!Directory.Exists("commands/")) Directory.CreateDirectory("commands/");
                FileManager.RegisterDirectory("Commands", FileManager.LoadDirectory("commands/"));

                // Cache
                services.GetRequiredService<Cache>();
                CacheThread cacheThread = services.GetRequiredService<CacheThread>();
                services.GetRequiredService<AutoCacher>();

                // Database here so that we get the cache before it so we can add the guild to it
                services.GetRequiredService<DataBasifier>();

                //GuildLookup guildLookup = services.GetRequiredService<GuildLookup>();
                //guildLookup.LoadGuilds();

                Thread thread = new Thread(CacheThread.InitiazeCacheThread);
                thread.Start(services);

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
            if (!(log.Exception is null) && Config.LogTraces)
            {
                DashCMD.WriteWarning(log.ToString());
            }
            else if (!(log.Exception is null))
            {
                DashCMD.WriteWarning(log.ToString(fullException: false));
            }
            else
            {
                DashCMD.WriteStandard(log.ToString());
            }
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            CommandServiceConfig commandConfig = new CommandServiceConfig();
            commandConfig.DefaultRunMode = RunMode.Async;

            return new ServiceCollection()
                // Discord.Net stuff
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(new CommandService(commandConfig))
                .AddSingleton<HttpClient>()
                // Config and internal stuff
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<DataBasifier>()
                .AddSingleton<CacheThread>()
                .AddSingleton<AutoCacher>()
                .AddSingleton<Cache>()
                .AddSingleton<Emojis>()
                // Database
                .AddSingleton<GuildLookup>()
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
