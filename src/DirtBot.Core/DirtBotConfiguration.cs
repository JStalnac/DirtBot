using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using System;

namespace DirtBot.Core
{
    /// <summary>
    /// Represents a configuration for <see cref="DirtBot"/>
    /// </summary>
    public sealed class DirtBotConfiguration
    {
        /// <summary>
        /// Configuration used for the bot client.
        /// </summary>
        public DiscordConfiguration DiscordConfiguration { internal get; set; } = new DiscordConfiguration()
        {
            UseInternalLogHandler = false
        };

        /// <summary>
        /// Configuration used for CommandsNext.
        /// </summary>
        public CommandsNextConfiguration CommandsNextConfiguration { internal get; set; } = new CommandsNextConfiguration() { };

        /// <summary>
        /// Configuration used for Interactivity.
        /// </summary>
        public InteractivityConfiguration InteractivityConfiguration { internal get; set; } = new InteractivityConfiguration() { };

        /// <summary>
        /// Sets the token used for identifying the bot. See <see cref="DiscordConfiguration.Token"/>
        /// </summary>
        public string Token { get => throw new NotImplementedException(); set => DiscordConfiguration.Token = value; }

        /// <summary>
        /// Sets the log level used for DSharpPlus and the bot.
        /// </summary>
        public LogLevel LogLevel
        {
            internal get => _level;
            set
            {
                _level = value;
                DiscordConfiguration.LogLevel = Utilities.LogLevelUtilities.DirtBotToDSharpPlus(value);
            }
        }

        LogLevel _level = LogLevel.Info;

        /// <summary>
        /// Sets the file that logs will be written to.
        /// </summary>
        public string LogFile { internal get; set; }

        /// <summary>
        /// Sets the Datetime format used.
        /// </summary>
        public string DateTimeFormat
        {
            internal get => _datetimeFormat;
            set
            {
                _datetimeFormat = value;
                DiscordConfiguration.DateTimeFormat = value;
            }
        }

        string _datetimeFormat;

        /// <summary>
        /// Sets the prefix used for commands.
        /// </summary>
        public string CommandPrefix { internal get; set; } = "dirt ";

        /// <summary>
        /// Sets the connection string for Redis. Leave empty for no connecting.
        /// </summary>
        public string RedisUrl { internal get; set; } = "";

        /// <summary>
        /// Sets the prefix resolver used. See <see cref="Core.PrefixResolverType"/>
        /// </summary>
        public PrefixResolverType PrefixResolverType { internal get; set; } = PrefixResolverType.Redis;
    }

    /// <summary>
    /// Represents the prefix resolver type used.
    /// </summary>
    public enum PrefixResolverType
    {
        /// <summary>
        /// Custom prefix resolver.
        /// </summary>
        Custom,
        /// <summary>
        /// Default prefix resolver using Redis.
        /// </summary>
        Redis
    }
}
