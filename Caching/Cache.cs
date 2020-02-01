using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dash.CMD;
using Discord;
using Discord.WebSocket;

namespace DirtBot.Caching
{
    public class Cache
    {
        List<CacheSave> caches = new List<CacheSave>();
        // List of cached objects by name which have varriables and stuff of dynamic type.
        public static Dictionary<string, Dictionary<string, dynamic>> CachedObjects = new Dictionary<string, Dictionary<string, dynamic>>();
        
        public Dictionary<string, dynamic> this[IMessage message] 
        {
            get 
            {
                try
                {
                    return CachedObjects[(message.Channel as SocketGuildChannel).Guild.Id.ToString()];
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
            }
        }
        public Dictionary<string, dynamic> this[ulong id] 
        {
            get => this[id.ToString()];
        }
        public Dictionary<string, dynamic> this[string name] 
        {
            get 
            {
                try
                {
                    return CachedObjects[name];
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
            }
        }

        public List<CacheSave> Caches 
        {
            get 
            {
                return caches; 
            }

        }

        /// <summary>
        /// Returns a CacheSave with the provided id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CacheSave> GetFromCacheAsync(ulong id)
        {
            foreach (CacheSave cache in caches)
            {
                if (cache.Id == id.ToString()) return cache;
            }
            return null;
        }
        public async Task<CacheSave> GetFromCacheAsync(SocketMessage message)
        {
            SocketGuildChannel channel = message.Channel as SocketGuildChannel;
            if (channel.Guild is null) return null;
            string id = channel.Guild.Id.ToString();

            return await GetFromCacheAsync(channel.Guild.Id);
        }

        /// <summary>
        /// Adds a CacheSave to the cache.
        /// </summary>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task AddToCacheAsync(CacheSave save)
        {
            foreach (CacheSave item in caches)
            {
                // The cache already contains this CacheSave. It's a dublicate.
                if (item.Id == save.Id) return;
            }
            caches.Add(save);
            DashCMD.WriteStandard($"{save.Name} has been added to cache!");
        }

        /// <summary>
        /// Removes a CacheSave from the cache by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task RemoveFromCacheAsync(string id)
        {
            foreach (CacheSave cacheSave in caches)
            {
                if (cacheSave.Id == id)
                {
                    caches.Remove(cacheSave);
                    DashCMD.WriteStandard($"{cacheSave.Name} has been removed from cache!");
                    return;
                }
            }
        }
        public async Task RemoveFromCacheAsync(ulong id)
        {
            await RemoveFromCacheAsync(id.ToString());
        }
    }
}
