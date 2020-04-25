using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace DirtBot
{
    public class DirtBot
    {
        public static DiscordClient Client { get; private set; }
        Logger logger;
        public LogLevel LogLevel { get; } = LogLevel.Debug;

        public async Task StartAsync()
        {
            logger = new Logger("DirtBot", LogLevel);
            
            #region Client configuration and initialization
            // Here's the bot configuration
            var config = new DiscordConfiguration()
            {
                TokenType = TokenType.Bot,
                UseInternalLogHandler = false,
                LogLevel = LogLevel,
            };

            // Safely setting the token.
            try
            {
                config.Token = Config.Token;
            }
            catch (ArgumentNullException) { /* Oh no the token is invalid oh faaa >:) */ }

            Client = new DiscordClient(config);
            #endregion

            Client.Ready += async (e) =>
            {
                logger.Info("Ready");
            };

            Client.MessageCreated += async (e) =>
            {
                // Message logging for debugging purposes
                string username = $"{e.Author.Username}#{e.Author.Discriminator}";
                string guild = e.Guild is null ? "DM" : e.Guild.Name;
                string channel = e.Channel.Name == "" ? "" : $"#{e.Channel.Name}";
                logger.Debug($"Message: {username}@{guild}{channel}:{e.Message.Content}");
            };

            Client.GuildAvailable += async (e) =>
            {
                logger.Debug($"Guild available: '{e.Guild.Name}'");
            };

            // Logs stack traces nicely with the DSharpPlus internal logger :)
            Client.ClientErrored += async (e) =>
            {
                new Logger("Exception Logger").Error(null, e.Exception);
            };

            Client.SocketErrored += async (e) =>
            {
                new Logger("Exception Logger").Error(null, e.Exception);
            };

            // File logging
            //Client.DebugLogger.LogMessageReceived += Logger.DebugLogger_LogMessageReceived;
            Client.DebugLogger.LogMessageReceived += (sender, e) =>
            {
                var l = new Logger(e.Application, LogLevel);
                l.Debug(e.Message, e.Exception);
            };

            // Connecting to Redis before initializing the modules so that the Ready events can use it
            ConnectionMultiplexer redis = null;
            try
            {
                logger.Info("Connecting to Redis");
                redis = ConnectionMultiplexer.Connect(Config.RedisUrl);
            }
            catch (RedisConnectionException ex)
            {
                logger.Error($"Failed to connect to Redis: {ex.Message}");
                Environment.Exit(-1);
            }
            catch (Exception ex)
            {
                logger.Critical("Failed to connect to Redis", ex);
                Environment.Exit(-1);
            }

            var services = new ServiceCollection()
                .AddSingleton<DiscordClient>()
                .AddSingleton(redis)
                .AddSingleton(this)
                .BuildServiceProvider();

            Client.UseCommandsNext(new CommandsNextConfiguration()
            {
                PrefixResolver = CommandHandler.GetPrefix,
                Services = services,
            });

            // Executed when Ctrl+C is pressed. "Get off there!" Makes the bot go offline instantly when it is off
            Console.CancelKeyPress += async (sender, e) =>
            {
                logger.Info("Received shutdown signal\nDisconnecting");
                await Client.DisconnectAsync();
            };

            // Connecting to Discord
            try
            {
                logger.Info("Connecting to Discord");
                await Client.ConnectAsync();
            }
            catch (Exception e)
            {
                // This is why we don't want to throw in the prefix setting part.
                logger.Error("Failed to connect to Discord.", e);
            }

            await Task.Delay(-1);
        }
    }
}
