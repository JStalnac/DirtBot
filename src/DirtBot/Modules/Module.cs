using Discord.Commands;
using Discord.WebSocket;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;
using System;

namespace DirtBot.Modules
{
    public abstract class Module : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// The DirtBot Discord client
        /// </summary>
        protected DiscordSocketClient Client { get => DirtBot.Client; }
        /// <summary>
        /// Redis connection
        /// </summary>
        protected ConnectionMultiplexer Redis { get => DirtBot.Redis; }
        /// <summary>
        /// DirtBot services
        /// </summary>
        protected IServiceProvider Services { get => DirtBot.Services; }
        /// <summary>
        /// Number of the database the storage is in.
        /// </summary>
        protected int Database { get; } = 0;

        /// <summary>
        /// The name of the module used internally in Redis etc.
        /// </summary>
        public string Name { get => GetType().FullName.ToLower(); }
        /// <summary>
        /// The name displayed on Discord
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Gets the storage of the module for the guild.
        /// </summary>
        /// <param name="guildId">The id of the guild</param>
        /// <returns></returns>
        protected IDatabaseAsync GetStorage(ulong guildId)
        {
            return Redis.GetDatabase(Database).WithKeyPrefix($"plugins:{Name}:{guildId}:");
        }

        /// <summary>
        /// Gets the storage of the module for the guild.
        /// </summary>
        /// <param name="guild">The guild</param>
        /// <returns></returns>
        protected IDatabaseAsync GetStorage(SocketGuild guild)
        {
            // k then
            if (guild is null)
                return null;

            return GetStorage(guild.Id);
        }
    }
}
