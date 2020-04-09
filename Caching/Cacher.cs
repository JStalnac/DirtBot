using DirtBot.Logging;
using DirtBot.Services;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DirtBot.Caching
{
    public class Cacher : ServiceBase
    {
        public static Dictionary<string, dynamic> DefaultKeys { get; } = new Dictionary<string, dynamic>()
        {
            // Services etc
            { "greetingCount", 4 },
            { "maxGreetCount", 4 },
            { "fCount", 4 },
            { "maxfCount", 4 },
            // Object values
            { "RemoveAfter", 300 },
            { "Remove", true },
            // CreationTime
            // Name
        };

        public Cacher(IServiceProvider services)
        {
            if (services is null)
            {
                Logger.Log($"Cache: Services do not exist!", true, foregroundColor: ConsoleColor.Red);
                Environment.Exit(1);
            }

            InitializeService(services);
            //Client.MessageReceived += AddMessageToCache;
        }

        /// <summary>
        /// Starts the cache thread
        /// </summary>
        /// <param name="services"></param>
        public static void InitiazeCacheThread()
        {
            Logger.Log("Cache starting!", true, foregroundColor: ConsoleColor.Cyan);
            Logger.Log($"Current update interval: {Config.CacheUpdateInterval}", foregroundColor: ConsoleColor.DarkGray);

            while (true)
            {
                DateTime currentTime = DateTime.Now;

                foreach (var key in Cache.Caches.Keys)
                {
                    // Loading the cached object here for efficiency, we won't have to get it again.
                    Dictionary<string, dynamic> cached = Cache.Caches[key];

                    try
                    {
                        // If the cache should be removed
                        if (cached["Remove"])
                        {
                            TimeSpan timeDifference = currentTime - cached["CreationTime"];

                            // Check if it is time to remove this object. Remove it also if the time is less than our update interval
                            if (timeDifference.TotalSeconds > cached["RemoveAfter"] || cached["RemoveAfter"] < Config.CacheUpdateInterval / 1000)
                            {
                                Cache.Caches.Remove(key);
                                // Log...
                                Logger.Log($"{key} (Name: '{cached["Name"]}') has been removed from cache!", true, foregroundColor: ConsoleColor.White);
                            }
                        }
                        else
                        {
                            // Object shouldn't be removed... Next one please!
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        // Oopsie...
                        Logger.Log($"Cache update failed!", true, exception: e, foregroundColor: ConsoleColor.Red);
                    }
                }

                Thread.Sleep(Config.CacheUpdateInterval);
            }
        }

        /// <summary>
        /// Adds a message to cache when it is received.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task AddMessageToCache(SocketMessage message)
        {
            // Filter system messages.
            if (message.Source != Discord.MessageSource.User) return;

            // Filter DM messages.
            if (!(message.Channel is SocketGuildChannel)) return;

            // Don't cache bot messages. No user interaction there. If it is a command it has been triggered by a user.
            if (message.Author.IsBot) return;

            // By now we have only real user messages on guilds.
            SocketGuildChannel guildChannel = message.Channel as SocketGuildChannel;

            // Add to cache

            // Check if the guild has been already added...
            if (Cache.Caches.ContainsKey(guildChannel.Guild.Id.ToString()))
            {
                // It has. Set the creation time again...
                Cache[message]["CreationTime"] = DateTime.Now;
                Logger.Log($"Remove time for '{guildChannel.Guild.Name}' (ID: {guildChannel.Guild.Id}) has been extended!", true, foregroundColor: ConsoleColor.White);
            }
            else
            {
                Cache.Caches.Add(guildChannel.Guild.Id.ToString(), new Dictionary<string, dynamic>());

                foreach (var key in DefaultKeys)
                {
                    Cache[message].Add(key.Key, key.Value);
                }

                // The time this has been created. Used for removing the cache after a while
                Cache[message].Add("CreationTime", DateTime.Now);
                Cache[message].Add("Name", guildChannel.Guild.Name);

                Logger.Log($"'{guildChannel.Guild.Name}' (ID: {guildChannel.Guild.Id}) has been added to cache!", true, foregroundColor: ConsoleColor.White);
            }
        }
    }
}
