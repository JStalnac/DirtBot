﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DirtBot.Logging;

namespace DirtBot.Caching
{
    class CacheThread
    {
        static Logger Logger;
        Config config;
        Cache cache;
        IServiceProvider services;

        /// <summary>
        /// Starts the cache thread
        /// </summary>
        /// <param name="services"></param>
        public static void InitiazeCacheThread(object services)
        {
            if (services is null)
            {
                Console.WriteLine("Cache: Services do not exist!");
                Environment.Exit(1);
            }

            CacheThread cacheThread = new CacheThread(services as IServiceProvider);
            cacheThread.StartCacheAsync().GetAwaiter().GetResult();
        }

        public CacheThread(IServiceProvider services) 
        {
            this.services = services;
            config = services.GetRequiredService<Config>();
            cache = services.GetRequiredService<Cache>();
            Logger = Logger.GetLogger(this);
        }

        private async Task StartCacheAsync() 
        {
            await Logger.InfoAsync("Cache starting!");
            while (true) 
            {
                Thread.Sleep(config.CacheUpdateInterval);
                await CacheUpdate();
            }
        }

        private async Task CacheUpdate()
        {
            DateTime currentTime = DateTime.Now;

            foreach (CacheSave cacheSave in cache.Caches)
            {
                TimeSpan timeDifference = currentTime - cacheSave.CreationTime;

                if (timeDifference.TotalSeconds > cacheSave.RemoveAfter)
                {
                    await cache.RemoveFromCacheAsync(cacheSave.Id);
                    return;
                }
            }
        }
    }
}
