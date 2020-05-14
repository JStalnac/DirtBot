using DirtBot.Core.Utilities;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
                .AddSingleton(Client)   // DiscordClient
                .AddSingleton(redis)    // ConnectionMultiplexer
                .AddSingleton(this)     // DirtBot
                .BuildServiceProvider();

            // Configure CommandsNext
            var commands = Client.UseCommandsNext(new CommandsNextConfiguration()
            {
                // Command execution handler
                PrefixResolver = async (msg) => 
                {
                    var db = redis.GetDatabase(0) as IDatabaseAsync;

                    string prefix = null;
                    if (!msg.Channel.IsPrivate)
                        prefix = await db.StringGetAsync($"guilds:{msg.Channel.GuildId}:prefix:prefix");

                    prefix = prefix == null ? Config.Prefix : prefix;
                    int prefixLenght = CommandsNextUtilities.GetStringPrefixLength(msg, prefix);
                    return prefixLenght;

                    /* TODO: Implement this. Module disabling
                    // Get the command that it will be
                    var cn = Client.GetCommandsNext();
                    var cmd = cn.FindCommand(msg.Content.Substring(prefixLenght), out var _);

                    // No command
                    if (cmd is null)
                        return CommandsNextUtilities.GetStringPrefixLength(msg, prefix);

                    logger.Info(cmd.QualifiedName);

                    // Check if the module is disabled
                    {
                        var disabledModules = await db.SetMembersAsync("disabled_modules");
                        if (disabledModules.Length == 0)
                            return prefixLenght;

                        var names = disabledModules.ToStringArray();

                        if (EnabledCheckerUtilities.IsEnabled(cmd.Parent, names))
                        {
                            return prefixLenght;
                        }
                        else
                        {
                            return -1;
                        }
                    }*/
                },
                Services = services,
                EnableDefaultHelp = true
            });

            commands.CommandErrored += async (e) =>
            {
                var cmdLog = new Logger("Commands", LogLevel);
                cmdLog.Warning($"Command failed: {e.Command?.QualifiedName}: {e.Context.Message.Content}", e.Exception);
            };

            //Load all modules
            logger.Info("Loading all modules...");
            var manager = services.GetRequiredService<ModuleManager>();
            var moduleTypes = new List<Type>();
            moduleTypes.AddRange(manager.LoadAllModules(GetType().Assembly));

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
