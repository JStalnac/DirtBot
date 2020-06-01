using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        readonly string token;
        readonly string redisUrl;
        readonly string commandPrefix;

        public DirtBot(string token, string redisUrl = "localhost", string commandPrefix = "!", LogLevel logLevel = LogLevel.Info, string logFile = "log.txt")
        {
            if (String.IsNullOrEmpty(token) || String.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException("Token cannot be null");
            if (String.IsNullOrEmpty(logFile) || String.IsNullOrWhiteSpace(logFile))
                throw new ArgumentException("Invalid log file.");

            this.token = token;
            this.redisUrl = redisUrl;
            this.commandPrefix = commandPrefix;
            LogLevel = logLevel;
            LogFile = logFile;
            Logger.SetLogFile(logFile);
        }

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
                Token = token
            };

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
                logger.Info($"Message: {username}@{guild}{channel}:{e.Message.Content}");
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
                redis = ConnectionMultiplexer.Connect(redisUrl);
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
            var commands = Client.UseCommandsNext(new CommandsNextConfiguration()
            {
                PrefixResolver = async (msg) =>
                {
                    var db = redis.GetDatabase(0) as IDatabaseAsync;

                    string prefix = null;
                    if (!msg.Channel.IsPrivate)
                        prefix = await db.StringGetAsync($"guilds:{msg.Channel.GuildId}:prefix:prefix");

                    prefix = prefix == null ? commandPrefix : prefix;
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

            // Command error handler, as you can see
            commands.CommandErrored += async (e) =>
            {
                var cmdLog = new Logger("Commands", LogLevel);
                cmdLog.Warning($"Command failed: {e.Command?.QualifiedName}: {e.Context.Message.Content}", e.Exception);

                var errorEmbed = new DiscordEmbedBuilder()
                    .WithTitle("**An error occured**")
                    .WithColor(DiscordColor.Red);

                if (e.Exception is CommandNotFoundException)
                    errorEmbed.WithDescription("Command not found");
                else if (e.Exception is ArgumentException)
                    errorEmbed.WithDescription($"Could not find a command with those arguments\nSee `help {e.Context.Command.QualifiedName}` for help");
                else if (e.Exception is AggregateException)
                    errorEmbed.WithDescription("The command failed. See log for details");
                else if (e.Exception is ChecksFailedException ex)
                {
                    var checks = ex.FailedChecks;
                    do
                    {
                        // Source checks
                        if (checks.FirstOrDefault(x => x is RequireDirectMessageAttribute) is RequireDirectMessageAttribute)
                        {
                            errorEmbed.WithDescription("This command must be executed in DMs only.");
                            break;
                        }
                        if (ex.FailedChecks.FirstOrDefault(x => x is RequireGuildAttribute) is RequireGuildAttribute)
                        {
                            errorEmbed.WithDescription("This command must be executed in a guild.");
                            break;
                        }

                        // Permission checks
                        if (checks.FirstOrDefault(x => x is RequireBotPermissionsAttribute) is RequireBotPermissionsAttribute botPerms)
                            errorEmbed.AddField("I need one or more of these permissions", botPerms.Permissions.ToPermissionString());
                        if (checks.FirstOrDefault(x => x is RequireUserPermissionsAttribute) is RequireUserPermissionsAttribute userPerms)
                            errorEmbed.AddField("You need one or more of these permissions", userPerms.Permissions.ToPermissionString());

                        // Required roles
                        if (checks.FirstOrDefault(x => x is RequireRolesAttribute) is RequireRolesAttribute roles)
                            errorEmbed.AddField("You need one of these roles to do that", String.Join(", ", roles.RoleNames));

                        // Owner
                        if (checks.FirstOrDefault(x => x is RequireOwnerAttribute) is RequireOwnerAttribute owner)
                        {
                            errorEmbed.WithDescription("You must be my owner to do that!");
                            break;
                        }

                        // Nsfw
                        if (checks.FirstOrDefault(x => x is RequireNsfwAttribute) is RequireNsfwAttribute nsfw)
                        {
                            errorEmbed.WithDescription("This command needs to be executed in an NSFW channel. No idea why");
                            break;
                        }
                    } while (false);

                    if (errorEmbed.Fields.Count == 0 && String.IsNullOrEmpty(errorEmbed.Description))
                    {
                        errorEmbed.WithDescription("Some check failed here so I can't allow you to do that. Please report this to my owner.");
                        cmdLog.Error($"A failed check was not handled. Failed Checks: {String.Join(", ", ex.FailedChecks)}");
                    }
                }
                else if (e.Exception != null)
                    errorEmbed.WithDescription("Internal error. See log for details");

                if (e.Exception != null)
                    await e.Context.RespondAsync(embed: errorEmbed.Build());
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
