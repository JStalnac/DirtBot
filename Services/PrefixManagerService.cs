using DirtBot.Database;
using DirtBot.Database.Models;
using DirtBot.Extensions;
using DirtBot.Logging;
using DirtBot.Services.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirtBot.Services
{
    public class PrefixManagerService
    {
        public string DefaultPrefix { get => options.DefaultPrefix; }

        private readonly IServiceProvider services;
        private readonly ConnectionMultiplexer redis;
        private readonly PrefixManagerOptions options;

        public PrefixManagerService(IServiceProvider services, ConnectionMultiplexer redis, IOptions<PrefixManagerOptions> options)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            this.services = services;
            this.redis = redis;
            this.options = options.Value;
        }

        /// <summary>
        /// Gets a prefix from the database.
        /// First tries to get a cached prefix from Redis, then proceeds to get it from MySql if it is not cached.
        /// Automatically caches prefixes for you. Lifetime's of old caches won't be extended.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public async Task<string> GetPrefixAsync(ulong? guild)
        {
            // Direct messages don't have custom prefixes
            if (!guild.HasValue)
                return options.DefaultPrefix;
            // Get a cached prefix from Redis
            var rdb = redis.GetDatabase(0);
            string result = await rdb.StringGetAsync($"prefixes:{guild}");

            // Key stored in Redis
            if (result != null)
                return result;

            // Get the prefix from MySql database;
            using (var db = services.GetRequiredService<DatabaseContext>())
                result = (await AsyncEnumerable.FirstOrDefaultAsync(db.Prefixes, p => p.Id == guild))?.Prefix;
            result ??= options.DefaultPrefix;
            CachePrefix((ulong)guild, result).Release();
            return result;
        }

        /// <summary>
        /// Sets the guilds prefix.
        /// Does not cache prefixes for you.
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public async Task SetPrefixAsync(ulong? guild, string prefix)
        {
            if (!guild.HasValue)
                return;
            if (String.IsNullOrEmpty(prefix?.TrimEnd()))
                throw new ArgumentNullException(nameof(prefix));
            if (prefix.Length > 30)
                throw new ArgumentException("Prefix must be less than 100 characters long", nameof(prefix));

            using (var db = services.GetRequiredService<DatabaseContext>())
            {
                var g = await ((IAsyncEnumerable<GuildPrefix>)db.Prefixes).FirstOrDefaultAsync(x => x.Id == guild);
                if (g is null)
                    await db.AddAsync(new GuildPrefix
                    {
                        Id = guild.Value,
                        Prefix = prefix
                    });
                else
                {
                    g.Prefix = prefix;
                    db.Entry(g).State = EntityState.Modified;
                }

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    Logger.GetLogger(this).Warning($"Failed to update prefix for guild {guild}. Prefix: '{prefix}'");
                }

                //string escaped = MySqlHelper.EscapeString(prefix);
                //int changed = await db.Database.ExecuteSqlRawAsync($"INSERT INTO prefixes(Id, Prefix) VALUES ({guild}, '{escaped}') ON DUPLICATE KEY UPDATE Prefix = '{escaped}'").ConfigureAwait(false);
                //if (changed == 0)
                //    Logger.GetLogger(this).Warning($"Updated prefix but zero rows changed in database. Guild: {guild} Prefix: {prefix}");
            }
        }

        /// <summary>
        /// Caches the prefix for the guild in Redis.
        /// </summary>
        /// <param name="guild">Guild</param>
        /// <param name="prefix">Prefix</param>
        /// <returns></returns>
        public async Task CachePrefix(ulong guild, string prefix)
        {
            // Cache the prefix for the guild in Redis.
            await redis.GetDatabase(0).StringSetAsync($"prefixes:{guild}", prefix, TimeSpan.FromMinutes(15),
                flags: CommandFlags.FireAndForget);
        }

        /// <summary>
        /// Restores the lifetime of a prefix in the cache if it exists.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public async Task RestoreCache(ulong guild)
        {
            await redis.GetDatabase(0).KeyExpireAsync($"prefixes:{guild}", TimeSpan.FromMinutes(15),
                flags: CommandFlags.FireAndForget);
        }

        public static string PrettyPrefix(string prefix)
        {
            if (String.IsNullOrEmpty(prefix?.Trim()))
                return "";
            return prefix.Contains(' ') ? $"'{prefix}'" : prefix;
        }
    }
}