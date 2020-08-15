using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirtBot.Services
{
    public class CategoryManagerService : ServiceBase
    {
        /// <summary>
        /// Gets if a category is disabled in a channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<bool> IsCategoryEnabled(IChannel channel, string category)
        {
            if (String.IsNullOrEmpty(category))
                throw new ArgumentNullException(nameof(category));
            if (channel is null)
                throw new ArgumentNullException(nameof(channel));

            // Globally disabled
            if ((await GetDisabledCategoriesGlobalAsync()).Any(x => x == category))
                return false;

            // Disabled in guild
            if (channel is IGuildChannel c)
                return (await GetDisabledCategoriesAsync(c.GuildId)).Any(x => x == category);

            // Private channel, not disabled
            return true;
        }

        /// <summary>
        /// Gets all the disabled modules from Redis.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public async Task<string[]> GetDisabledCategoriesAsync(ulong guild)
        {
            var db = Redis.GetDatabase(1);
            var result = await db.SetMembersAsync($"disabled_modules:{guild}");
            return new List<string>(result.Select(x => x.ToString())).ToArray();
        }

        /// <summary>
        /// Gets the globally disabled modules from Redis.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public async Task<string[]> GetDisabledCategoriesGlobalAsync()
        {
            var rdb = Redis.GetDatabase(1);
            var result = await rdb.SetMembersAsync($"disabled_modules");
            return new List<string>(result.Select(x => x.ToString())).ToArray();
        }

        /// <summary>
        /// Adds a module to disabled modules for a guild disabling it.
        /// Returns True if the module was not present and was added.
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public Task<bool> AddDisabledCategoryAsync(ulong guild, string category)
        {
            if (String.IsNullOrEmpty(category?.TrimEnd()))
                throw new ArgumentNullException(nameof(category));

            var db = Redis.GetDatabase(1);
            return db.SetAddAsync($"disabled_modules:{guild}", category);
        }

        /// <summary>
        /// Adds a module to disabled modules for all guilds and users disabling globally it.
        /// Returns True if the module was not present and added.
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public Task<bool> AddDisabledCategoryGlobalAsync(string category)
        {
            if (String.IsNullOrEmpty(category?.TrimEnd()))
                throw new ArgumentNullException(nameof(category));

            Logger.GetLogger(this).Important($"Module disabled globally: {category}");
            var db = Redis.GetDatabase(1);
            return db.SetAddAsync($"disabled_modules", category);
        }

        /// <summary>
        /// Removes a module from the disabled modules for a guild enabling it.
        /// Returns True if the module was present and was removed.
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public Task<bool> RemoveDisabledCategoryAsync(string category)
        {
            if (String.IsNullOrEmpty(category?.Trim()))
                throw new ArgumentNullException(nameof(category));

            Logger.GetLogger(this).Important($"Module enabled globally: {category}");
            var db = Redis.GetDatabase(1);
            return db.SetRemoveAsync($"disabled_modules", category);
        }

        /// <summary>
        /// Removes a module from the disabled modules for all guilds and users enabling it globally.
        /// Returns True if the module was present.
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public Task<bool> RemoveDisabledCategoryGlobalAsync(ulong guild, string category)
        {
            if (String.IsNullOrEmpty(category?.Trim()))
                throw new ArgumentNullException(nameof(category));

            var db = Redis.GetDatabase(1);
            return db.SetRemoveAsync($"disabled_modules:{guild}", category);
        }
    }
}
