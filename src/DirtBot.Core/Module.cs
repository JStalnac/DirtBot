using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DirtBot.Core
{
    public abstract class Module : IModule
    {
        private Configuration configuration = null;

        /// <summary>
        /// The name of this module used internally in Redis etc.
        /// If you want to use same storage with another module use the same name as it.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The name displayed for users.
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Logger for this module.
        /// </summary>
        protected Logger Log { get; set; }

        /// <summary>
        /// The current enabled modules.
        /// </summary>
        protected IReadOnlyList<IModule> Modules { get => Services.GetRequiredService<ModuleManager>().Modules; }

        /// <summary>
        /// DirtBot's Discord client
        /// </summary>
        protected DiscordClient Client { get => Services.GetRequiredService<DiscordClient>(); }

        /// <summary>
        /// The current DirtBot instance.
        /// </summary>
        protected DirtBot DirtBot { get => Services.GetRequiredService<DirtBot>(); }

        /// <summary>
        /// Redis connection
        /// </summary>
        protected ConnectionMultiplexer Redis { get => Services.GetRequiredService<ConnectionMultiplexer>(); }

        /// <summary>
        /// DirtBot services.
        /// </summary>
        protected IServiceProvider Services { get; }

        /// <summary>
        /// Initializes the module.
        /// </summary>
        /// <param name="services"></param>
        public Module(IServiceProvider services)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            Services = services;
            Log = Logger.GetLogger(this, DirtBot.LogLevel);
        }

        /// <summary>
        /// Gets a database prefixed to the the storage of this module.
        /// <para></para>
        /// The prefix is <c>modules:{Name}:</c>
        /// </summary>
        /// <returns></returns>
        public IDatabase GetStorage()
        {
            return Redis.GetDatabase(0).WithKeyPrefix($"modules:{Name}:");
        }

        /// <summary>
        /// Gets a database prefixed to the the storage of this module for a guild.
        /// <para></para>
        /// The prefix is <c>guilds:{id}:{Name}:</c>
        /// </summary>
        /// <param name="guildId">The id of the guild</param>
        /// <returns></returns>
        protected IDatabase GetStorage(ulong guildId)
        {
            return Redis.GetDatabase(0).WithKeyPrefix($"guilds:{guildId}:{Name}:");
        }

        /// <summary>
        /// Gets a database prefixed to the storage of this module for a guild.
        /// <para></para>
        /// See <see cref="GetStorage(ulong)"/>
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        protected IDatabase GetStorage(DiscordGuild guild)
        {
            // k then
            if (guild is null)
                return null;

            return GetStorage(guild.Id);
        }

        // TODO: Better naming
        /// <summary>
        /// Gets a database prefixed to the storage root of a guild.
        /// <para></para>
        /// The prefix is <c>guilds:{id}:</c>
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public IDatabase GetGuildStorage(DiscordGuild guild)
        {
            if (guild is null)
                return null;
            return Redis.GetDatabase(0).WithKeyPrefix($"guilds:{guild.Id}:");
        }

        public Configuration GetConfiguration()
        {
            if (configuration is null)
                configuration = Configuration.LoadConfiguration($"modules/config/{Name}/config.yml");
            return configuration;
        }

        /// <summary>
        /// Returns True if this module is enabled in the specified guild.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        protected async Task<bool> IsEnabled(DiscordGuild guild)
        {
            var db = GetGuildStorage(guild);
            var disabledModules = await db.SetMembersAsync("disabled_modules");

            foreach (var s in disabledModules)
            {
                if (s == Name)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
