using DirtBot.Database;
using DirtBot.Logging;
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
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DirtBot
{
    internal class Program
    {
        public static IServiceProvider Services { get; private set; }
        public static ConnectionMultiplexer Redis { get; private set; }

        public static async Task Main(string[] args)
        {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");

            // Enable file output
            Logger.LogFile = LogFileUpdaterService.GetLogFile(DateTime.Now);
            Logger.UseTypeFullName = true;

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
            catch (OptionsValidationException ex)
            {
                log.Critical($"Option {ex.OptionsType} failed to validate: {String.Join('\n', ex.Failures)}");
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
                .ConfigureLogging(o =>
                {
                    o.ClearProviders();
                })
                .ConfigureDiscordHost<DiscordSocketClient>((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        ExclusiveBulkDelete = true,
                        LogLevel = Discord.LogSeverity.Debug
                    };
                    config.Token = context.Configuration["Token"];
                })
                .UseCommandService((context, config) =>
                {
                    config.DefaultRunMode = Discord.Commands.RunMode.Async;
                })
                .ConfigureServices((c, services) =>
                {
                    // Set log level from appsettings
                    Enum.TryParse(typeof(Logging.LogLevel), c.Configuration["LogLevel"], out object logLevel);
                    Logger.MinimumLogLevel = logLevel != null ? (Logging.LogLevel)logLevel : Logging.LogLevel.Info;

                    // Connect to Redis
                    var logger = Logger.GetLogger<Program>();
                    logger.Info("Configuring services...");

                    // Database
                    services.AddDbContextPool<DatabaseContext>(options => options.UseSqlite(c.Configuration.GetConnectionString("Sqlite")));

                    ConnectionMultiplexer redis = null;
                    try
                    {
                        string connectionString = c.Configuration.GetConnectionString("Redis");
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

                    var section = c.Configuration.GetSection("Services");

                    // Required services
                    services.AddSingleton<CategoryManagerService>();
                    services.AddSingleton<PrefixManagerService>()
                    .Configure<PrefixManagerOptions>(options =>
                    {
                        options.DefaultPrefix = c.Configuration["DefaultPrefix"];
                    });
                    services.AddSingleton<HelpProviderService>();
                    services.AddHostedService<CommandHandlerService>();
                    services.AddHostedService<LogFileUpdaterService>()
                    .Configure<LogFileUpdaterOptions>(section.GetSection("LogFileUpdater"));

                    // Other services
                    services.AddHostedService<CustomStatusService>()
                    .Configure<CustomStatusServiceOptions>(section.GetSection("CustomStatusService"));
                    services.AddHostedService<LoggingService>();

                    logger.Info("Services configured");
                });
    }
}