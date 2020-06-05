using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;
using System;
using System.Collections.Generic;

namespace DirtBot.Core
{
    /// <summary>
    /// Provides extensions that allow <see cref="CommandContext"/> to be used like <see cref="Module"/>
    /// </summary>
    public static class CommandContextExtensions
    {
        /// <summary>
        /// The current enabled modules.
        /// </summary>
        public static IReadOnlyList<IModule> GetModules(this CommandContext ctx) => ctx.Services.GetRequiredService<ModuleManager>().Modules;

        /// <summary>
        /// The current DirtBot instance.
        /// </summary>
        public static DirtBot GetDirtBot(this CommandContext ctx) => ctx.Services.GetRequiredService<DirtBot>();

        /// <summary>
        /// Gets a logger using the provided application with DirtBot's log level.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="application"></param>
        /// <returns></returns>
        public static Logger GetLogger(this CommandContext ctx, string application) => new Logger(application, GetDirtBot(ctx).LogLevel);
        
        /// <summary>
        /// Redis connection
        /// </summary>
        public static ConnectionMultiplexer GetRedis(this CommandContext ctx)
        {
            var redis = ctx.Services.GetService<ConnectionMultiplexer>();
            if (redis is null)
                throw new InvalidOperationException("Redis is not connected.");
            return redis;
        }

        /// <summary>
        /// Gets a database prefixed to the the storage of this module.
        /// <para></para>
        /// The prefix is <c>modules:{Name}:</c>
        /// </summary>
        /// <returns></returns>
        public static IDatabase GetModuleStorage(this CommandContext ctx, string name) => GetRedis(ctx).GetDatabase(0).WithKeyPrefix($"modules:{name}:");

        /// <summary>
        /// Gets a database prefixed to the the storage of this module for the guild.
        /// <para></para>
        /// The prefix is <c>guilds:{id}:{Name}:</c>
        /// </summary>
        /// <param name="name">The internal name of a module.</param>
        /// <returns></returns>
        public static IDatabase GetStorage(this CommandContext ctx, string name)
        {
            return GetRedis(ctx).GetDatabase(0).WithKeyPrefix($"guilds:{ctx.Guild.Id}:{name}:");
        }
        
        /// <summary>
        /// Gets a database prefixed to the storage root of the guild.
        /// <para></para>
        /// The prefix is <c>guilds:{id}:</c>
        /// </summary>
        /// <returns></returns>
        public static IDatabase GetGuildStorage(this CommandContext ctx)
        {
            if (ctx.Guild is null)
                return null;
            return GetRedis(ctx).GetDatabase(0).WithKeyPrefix($"guilds:{ctx.Guild.Id}:");
        }

        public static Configuration GetConfiguration(this CommandContext ctx, string name) => Configuration.LoadConfiguration($"modules/config/{name}/config.yml");
    }
}
