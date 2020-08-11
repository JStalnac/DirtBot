using System;
using System.Linq;
using System.Threading.Tasks;
using DirtBot.Database;
using DirtBot.Extensions;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using StackExchange.Redis;

namespace DirtBot.Services
{
    public class PrefixManagerService : ServiceBase
    {
        public string DefaultPrefix { get; private set; }

        public void Initialize(string defaultPrefix)
        {
            if (String.IsNullOrEmpty(defaultPrefix?.TrimEnd()))
                throw new ArgumentNullException(nameof(defaultPrefix));
            DefaultPrefix = defaultPrefix;
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
                return DefaultPrefix;
            // Get a cached prefix from Redis
            var rdb = Redis.GetDatabase(0);
            string result = await rdb.StringGetAsync($"prefixes:{guild}");

            // Key stored in Redis
            if (result != null)
                return result;

            // Get the prefix from MySql database;
            using (var db = new DatabaseContext())
                result = (await AsyncEnumerable.FirstOrDefaultAsync(db.Prefixes, p => p.Id == guild))?.Prefix;
            result ??= DefaultPrefix;
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

            using (var db = new DatabaseContext())
            {
                string escaped = MySqlHelper.EscapeString(prefix);
                int changed = await db.Database.ExecuteSqlRawAsync($"INSERT INTO prefixes(Id, Prefix) VALUES ({guild}, '{escaped}') ON DUPLICATE KEY UPDATE Prefix = '{escaped}'").ConfigureAwait(false);
                if (changed == 0)
                    Logger.GetLogger(this).Warning($"Updated prefix but zero rows changed in database. Guild: {guild} Prefix: {prefix}");
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
            await Redis.GetDatabase(0).StringSetAsync($"prefixes:{guild}", prefix, TimeSpan.FromMinutes(15),
                flags: CommandFlags.FireAndForget);
        }

        /// <summary>
        /// Restores the lifetime of a prefix in the cache if it exists.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public async Task RestoreCache(ulong guild)
        {
            await Redis.GetDatabase(0).KeyExpireAsync($"prefixes:{guild}", TimeSpan.FromMinutes(15),
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