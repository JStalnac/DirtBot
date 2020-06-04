using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
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
        public LogLevel LogLevel { get; }
        public string LogFile { get; }
        internal DiscordClient Client { get; private set; }
        Logger logger;
        bool active = false;
        readonly DirtBotConfiguration config;

        public DirtBot(DirtBotConfiguration config)
        {
            this.config = config;

            // Messes up the log if enabled.
            config.DiscordConfiguration.UseInternalLogHandler = false;

            Client = new DiscordClient(config.DiscordConfiguration);

            LogLevel = config.LogLevel;
            LogFile = config.LogFile;
            Logger.SetLogFile(config.LogFile);
        }

        public async Task StartAsync()
        {
            if (active)
                throw new InvalidOperationException("This instance is already in use.");
            active = true;

            logger = new Logger("DirtBot", LogLevel);
            logger.Info("Starting DirtBot!");

            Client.Ready += async (e) => logger.Info("Ready");

            Client.MessageCreated += async (e) =>
            {
                // Message logging for debugging purposes
                string username = $"{e.Author.Username}#{e.Author.Discriminator}";
                string guild = e.Guild is null ? "DM" : e.Guild.Name;
                string channel = e.Channel.Name == "" ? "" : $"#{e.Channel.Name}";
                logger.Info($"Message: {username}@{guild}{channel}:{e.Message.Content}");
            };

            Client.GuildAvailable += async (e) => logger.Debug($"Guild available: '{e.Guild.Name}'");

            // DSharpPlus messages
            Client.DebugLogger.LogMessageReceived += (sender, e) =>
            {
                var l = new Logger(e.Application, LogLevel);
                l.Write(e.Message, Utilities.LogLevelUtilities.DSharpPlusToDirtBot(e.Level), e.Exception);
            };

            // Connecting to Redis before initializing the modules so that the Ready events can use it.
            ConnectionMultiplexer redis = null;
            if (!String.IsNullOrEmpty(config.RedisUrl))
            {
                try
                {
                    logger.Info("Connecting to Redis");
                    redis = ConnectionMultiplexer.Connect(config.RedisUrl);
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
            }

            // Interactivity
            Client.UseInteractivity(new InteractivityConfiguration());

            // Configure services
            var services = new ServiceCollection()
                .AddSingleton<ModuleManager>()
                .AddSingleton(Client)   // DiscordClient
                .AddSingleton(redis)    // ConnectionMultiplexer
                .AddSingleton(this)     // DirtBot
                .BuildServiceProvider();

            // Configure CommandsNext
            if (config.PrefixResolverType == PrefixResolverType.Redis)
                config.CommandsNextConfiguration.PrefixResolver = async (msg) =>
                {
                    var db = redis.GetDatabase(0) as IDatabaseAsync;

                    string prefix = null;
                    if (!msg.Channel.IsPrivate)
                        prefix = await db.StringGetAsync($"guilds:{msg.Channel.GuildId}:prefix:prefix");

                    prefix = prefix ?? config.CommandPrefix;
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
                };

            config.CommandsNextConfiguration.Services = services;
            var commands = Client.UseCommandsNext(config.CommandsNextConfiguration);

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
            foreach (var file in Directory.EnumerateFiles("modules", "*.dll"))
            {
                var a = Assembly.LoadFrom(file);
                moduleTypes.AddRange(manager.LoadAllModules(a));
            }

            logger.Info("Initializing modules...");
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
                logger.Error("Failed to connect to Discord.", e);
            }

            await Task.Delay(-1);
        }
    }
}
