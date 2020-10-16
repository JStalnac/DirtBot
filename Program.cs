using DirtBot.Database;
using DirtBot.Services;
using DirtBot.Services.Options;
using DirtBot.Translation;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DirtBot
{
    internal class Program
    {
        public static IServiceProvider Services { get; private set; }
        public static ConnectionMultiplexer Redis { get; private set; }
        public static IConfigurationRoot Configuration { get; private set; }

        public static async Task Main(string[] args)
        {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");
            string logFile = $"logs/{DateTimeOffset.Now.ToUnixTimeSeconds()}.log";

            // Enable file output
            Logger.LogFile = logFile;

            var log = Logger.GetLogger("Main");
            log.Important("Starting! Hello World!");

            try
            {
                using var host = CreateHostBuilder(args).Build();
                if (Redis is null)
                {
                    log.Critical("Redis is not connected.");
                    return;
                }
                Services = host.Services;
                log.Info("Starting host...");
                await host.StartAsync();
                log.Info("Host started");
                await host.WaitForShutdownAsync();
            }
            catch (Exception ex)
            {
                log.Critical("The application threw an unhandled exception.", ex);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(x =>
                {
                    x.SetBasePath(Directory.GetCurrentDirectory())
                        .AddCommandLine(args)
                        .AddEnvironmentVariables()
                        .AddJsonFile("appsettings.json");
                })
                .ConfigureLogging(options =>
                {
                    options.ClearProviders();
                })
                .ConfigureDiscordHost<DiscordSocketClient>((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        ExclusiveBulkDelete = true
                    };
                    config.Token = context.Configuration["Token"];
                })
                .UseCommandService((context, config) =>
                {
                    config.DefaultRunMode = Discord.Commands.RunMode.Async;
                })
                .ConfigureServices((context, services) =>
                {
                    // Connect to Redis
                    var logger = Logger.GetLogger<Program>();
                    logger.Info("Configuring services...");

                    // Database
                    services.AddDbContextPool<DatabaseContext>(options => options.UseSqlite(context.Configuration.GetConnectionString("Sqlite")));

                    ConnectionMultiplexer redis = null;
                    try
                    {
                        string connectionString = context.Configuration.GetConnectionString("Redis");
                        if (String.IsNullOrEmpty(connectionString))
                            logger.Critical("Redis connection string not specified");
                        else
                        {
                            logger.Info("Connecting to Redis");
                            redis = ConnectionMultiplexer.Connect(connectionString);
                            logger.Info("Connected to Redis");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Critical("Failed to connect to Redis", ex);
                    }

                    if (redis is null)
                    {
                        // So that you can migrate EF without a Redis connection.
                        logger.Critical("Redis is not connected so other services will not be loaded.");
                        return;
                    }

                    Redis = redis;
                    services.AddSingleton(redis);

                    logger.Info("Loading translations...");
                    TranslationManager.LoadTranslations().Wait();
                    logger.Info("Translations loaded");

                    // Required services
                    services.AddSingleton<CategoryManagerService>();
                    services.AddSingleton<PrefixManagerService>();
                    services.Configure<PrefixManagerOptions>(options =>
                    {
                        options.DefaultPrefix = context.Configuration["DefaultPrefix"];
                    });
                    services.AddSingleton<HelpProviderService>();
                    services.AddHostedService<CommandHandlerService>();

                    // Other services
                    services.AddHostedService<CustomStatusService>();
                    services.AddHostedService<LoggingService>();

                    logger.Info("Services configured");
                });
    }
}