using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DirtBot.Database;
using DirtBot.Services;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Color = System.Drawing.Color;
using ServerType = Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType;

namespace DirtBot
{
    // Named like this to solve naming problems
    public class Dirtbot
    {
        public static DiscordSocketClient Client { get; private set; }
        public static ConnectionMultiplexer Redis { get; private set; }
        public static IServiceProvider Services { get; private set; }
        private static Configuration Configuration { get; set; }

        /// <summary>
        /// All services that should be added to the bot can be added here for ease-of-use.
        /// Boolean represents if the service should be initialized on start up.
        /// </summary>
        public static Dictionary<Type, bool> ServiceTypes { get; } = new Dictionary<Type, bool>()
        {
            { typeof(CustomStatusService), true }
        };

        public async Task StartAsync()
        {
            // Bot configuration
            Configuration = Configuration.LoadConfiguration("config.yml");
            Configuration.AddDefaultValue("token", "");
            Configuration.AddDefaultValue("prefix", "dirt ");
            Configuration.AddDefaultValue("redis_url", "localhost");
            Configuration.Save();

            using (var s = ConfigureServices())
            {
                Services = s;
                Client = Services.GetRequiredService<DiscordSocketClient>();

                // Log used for different operations in this method
                Logger log;

                // Discord logging
                Client.Log += LogAsync;
                Services.GetRequiredService<CommandService>().Log += LogAsync;

                // Redis storage
                log = Logger.GetLogger("Redis");
                string redisUrl = (string)Configuration.GetValue("redis_url");
                if (!String.IsNullOrEmpty(redisUrl.TrimEnd()))
                {
                    log.Info("Logging into Redis...");
                    try
                    {
                        Redis = ConnectionMultiplexer.Connect((string) Configuration.GetValue("redis_url"));
                    }
                    catch (RedisConnectionException e)
                    {
                        log.Critical("Failed to connect to Redis. Check the Redis address and that the server is online and try again.", e);
                        Environment.Exit(-1);
                    }
                    log.Info("Successfully connected to Redis.");
                }
                else
                    log.Info("Not connecting to Redis because no connection string was provided.\nSome services may not function correctly");

                // Initializing services
                // Commands
                await Services.GetRequiredService<CommandHandlerService>()
                    .InitializeAsync((string) Configuration.GetValue("prefix"));

                // Loading services added to ServiceTypes
                log = Logger.GetLogger("Services");
                foreach (var kvp in ServiceTypes)
                {
                    // Should not initialize
                    if (!kvp.Value) continue;
                    var t = kvp.Key;
                    log.Write($"Initializing service {t.Name}", Color.Gray);
                    try
                    {
                        Services.GetRequiredService(t);
                    }
                    catch (TargetInvocationException e)
                    {
                        log.Warning(
                            $"Failed to initialize service {t.Name} because its constructor threw an exception.",
                            e);
                    }
                    catch (MemberAccessException)
                    {
                        log.Warning($"Failed to initialize service {t.Name} because it doesn't have a valid public constructor.");
                    }
                    catch (Exception e)
                    {
                        log.Error($"Failed to initialize service {t.Name}", e);
                    }
                }

                // Login
                log = Logger.GetLogger("DirtBot");
                try
                {
                    log.Info("Logging onto Discord");
                    await Client.LoginAsync(TokenType.Bot, (string) Configuration.GetValue("token"));
                }
                catch (Discord.Net.HttpException)
                {
                    // Token is invalid
                    log.Critical("Invalid token. Terminating");
                    Environment.Exit(-1);
                }
                catch (Exception e)
                {
                    log.Critical($"Failed to connect to Discord ({e.Message})", e);
                    Environment.Exit(-1);
                }

                // Start the client
                await Client.StartAsync();

                // Making sure we won't exit the application so that the bot can stay online
                await Task.Delay(-1);
            }
        }

        private Task LogAsync(LogMessage message)
        {
            var log = Logger.GetLogger(message.Source);
            // Select correct log level
            var level = LogLevel.Info;
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    level = LogLevel.Critical;
                    break;
                case LogSeverity.Error:
                    level = LogLevel.Error;
                    break;
                case LogSeverity.Warning:
                    level = LogLevel.Warning;
                    break;
                case LogSeverity.Info:
                    break;
                case LogSeverity.Verbose:
                    level = LogLevel.Info;
                    break;
                case LogSeverity.Debug:
                    level = LogLevel.Debug;
                    break;
            }

            log.Write(message.Message, level, message.Exception);
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                // Discord.Net stuff
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {

                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton<HttpClient>()
                // Config and internal stuff
                .AddSingleton<CommandHandlerService>()
                // Database context
                .AddDbContext<DatabaseContext>();

            // Easy to add new services
            foreach (var kvp in ServiceTypes)
                services.AddSingleton(kvp.Key);
            return services.BuildServiceProvider();
        }
    }
}