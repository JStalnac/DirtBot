using DirtBot.Internal;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

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

        private HashSet<Type> commandModules = new HashSet<Type>();
        private HashSet<Assembly> commandAssemblies = new HashSet<Assembly>();

        private HashSet<Type> modules = new HashSet<Type>();
        private HashSet<Assembly> moduleAssemblies = new HashSet<Assembly>();

        public DirtBot(DirtBotConfiguration config)
        {
            this.config = config;

            // Messes up the log if enabled.
            config.DiscordConfiguration.UseInternalLogHandler = false;

            // Client
            Client = new DiscordClient(config.DiscordConfiguration);

            // These will be initialized first
            AddCommands<PrefixModule>();

            // Logging
            LogLevel = config.LogLevel;
            LogFile = config.LogFile;
            Logger.SetLogFile(config.LogFile);
        }

        public void AddCommands<T>() where T : BaseCommandModule
        {
            commandModules.Add(typeof(T));
        }

        public void AddCommands(Assembly assembly)
        {
            commandAssemblies.Add(assembly);
        }

        public void AddModule<T>() where T : Module
        {
            modules.Add(typeof(T));
        }

        public void AddModules(Assembly assembly)
        {
            moduleAssemblies.Add(assembly);
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
            else
                logger.Info("Not connecting to Redis because no connection string was provided.");

            MySqlConnection mysql = null;
            if (!String.IsNullOrEmpty(config.MySqlUrl))
            {
                try
                {
                    logger.Info("Connecting to MySql database");
                    mysql = new MySqlConnection(config.MySqlUrl);
                    mysql.Open();
                    logger.Info("Connected to MySql database");
                }
                catch (MySqlException ex)
                {
                    logger.Error($"Failed to connect to MySql: {ex.Message}");
                    Environment.Exit(-1);
                }
                catch (Exception ex)
                {
                    logger.Critical("Failed to connect to MySql", ex);
                    Environment.Exit(-1);
                }
            }
            else
                logger.Info("Not connecting to MySql because no connection string was provided.");

            // Interactivity
            Client.UseInteractivity(config.InteractivityConfiguration);

            // Configure services
            var sb = new ServiceCollection()
                .AddSingleton<ModuleManager>()
                .AddSingleton(Client)   // DiscordClient
                .AddSingleton(this);     // DirtBot

            if (redis != null)
                sb.AddSingleton(redis);     // ConnectionMultiplexer

            if (mysql != null)
                sb.AddSingleton(mysql);     // MySqlConnection

            var services = sb.BuildServiceProvider();

            // Configure CommandsNext
            if (config.PrefixResolverType == PrefixResolverType.Redis)
                config.CommandsNextConfiguration.PrefixResolver = async (msg) =>
                {
                    if (redis != null)
                    {
                        var db = redis.GetDatabase(0) as IDatabaseAsync;

                        string prefix = null;
                        if (!msg.Channel.IsPrivate)
                            prefix = await db.StringGetAsync($"guilds:{msg.Channel.GuildId}:prefix:prefix");

                        prefix = prefix ?? config.CommandPrefix;
                        int prefixLenght = CommandsNextUtilities.GetStringPrefixLength(msg, prefix);
                        return prefixLenght;
                    }
                    else
                        // No database connected.
                        return CommandsNextUtilities.GetStringPrefixLength(msg, config.CommandPrefix);

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

            // Internal
            foreach (var m in manager.LoadAllModules(GetType().Assembly))
                modules.Add(m);

            // Others
            foreach (var a in moduleAssemblies)
                foreach (var m in manager.LoadAllModules(a))
                    modules.Add(m);

            logger.Info("Initializing modules...");

            manager.InstallAllModules(modules.ToArray());

            // Internal commands are added in the constructor.
            foreach (var ca in commandAssemblies)
                foreach (var t in ca.GetTypes())
                    commandModules.Add(t);

            foreach (var cm in commandModules)
            {
                var log = new Logger("Command Loader");

                try
                {
                    if (typeof(BaseCommandModule).IsAssignableFrom(cm))
                        commands.RegisterCommands(cm);
                }
                catch (ArgumentNullException)
                {
                    log.Warning($"Failed to load commands from {cm.FullName} because it doesn't contain any commands.");
                }
                catch (MissingMethodException ex)
                {
                    // The type doesn't have a public contructor.
                    log.Error($"Failed to load commands from {cm.FullName} because it doesn't have a public constructor.");
                    log.Debug(null, ex);
                }
                catch (TargetInvocationException ex)
                {
                    // The module constructor threw an exception.
                    log.Warning($"Constructor for type {cm.FullName} failed.", ex.InnerException);
                }
                catch (Exception ex)
                {
                    log.Critical($"Failed to load module {cm.FullName}.", ex);
                }
            }

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
        }
    }
}
