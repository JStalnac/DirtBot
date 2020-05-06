using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DirtBot.Core
{
    public class DirtBot
    {
        public LogLevel LogLevel { get; } = LogLevel.Debug;
        internal DiscordClient Client { get; private set; }
        Logger logger;

        public async Task StartAsync()
        {
            logger = new Logger("DirtBot", LogLevel);
            logger.Info("Starting DirtBot!");

            #region Client configuration and initialization
            // Here's the bot configuration
            var config = new DiscordConfiguration()
            {
                TokenType = TokenType.Bot,
                UseInternalLogHandler = false,
                LogLevel = Utilities.LogLevelUtilities.DirtBotToDSharpPlus(LogLevel),
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

            // File logging
            Client.DebugLogger.LogMessageReceived += (sender, e) =>
            {
                var l = new Logger(e.Application, LogLevel);
                l.Write(e.Message, Utilities.LogLevelUtilities.DSharpPlusToDirtBot(e.Level), e.Exception);
            };

            // Connecting to Redis before initializing the modules so that the Ready events can use it
            ConnectionMultiplexer redis = null;
            try
            {
                logger.Info("Connecting to Redis");
                redis = ConnectionMultiplexer.Connect(Config.RedisUrl);
                logger.Info("Connected to Redis");
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

            // Configure services
            var services = new ServiceCollection()
                .AddSingleton<ModuleManager>()
                .AddSingleton<CommandHandler>()
                .AddSingleton(Client)   // DiscordClient
                .AddSingleton(redis)    // ConnectionMultiplexer
                .AddSingleton(this)     // DirtBot
                .BuildServiceProvider();

            // Configure CommandsNext
            var commands = Client.UseCommandsNext(new CommandsNextConfiguration()
            {
                PrefixResolver = CommandHandler.GetPrefix,
                Services = services,
                UseDefaultCommandHandler = false
            });

            //Load all modules
            logger.Info("Loading all modules...");
            var manager = services.GetRequiredService<ModuleManager>();
            var moduleTypes = new List<Type>();

            // Other modules
            foreach (var file in Directory.EnumerateFiles("Modules", "*.dll"))
            {
                var a = Assembly.LoadFrom(file);
                moduleTypes.AddRange(manager.LoadAllModules(a));
            }

            manager.InstallAllModules(moduleTypes.ToArray());
            logger.Info("Loaded all modules!");
            
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
