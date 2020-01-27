using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dash.CMD;
using Discord.WebSocket;

namespace DirtBot.Caching
{
    public class Cache
    {
        List<CacheSave> caches = new List<CacheSave>();

        public Cache() { }

        public List<CacheSave> Caches 
        {
            get { return caches; }
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

            foreach (CacheSave cache in caches)
            {
                if (cache.Id == id) return cache;
            }
            return null;
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
            foreach (CacheSave cacheSave in caches)
            {
                if (cacheSave.Id == id.ToString()) 
                {
                    caches.Remove(cacheSave);
                    DashCMD.WriteStandard($"{cacheSave.Name} has been removed from cache!");
                    return;
                }
            }
        }
    }
}
