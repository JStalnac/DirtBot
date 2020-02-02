using System;
using DirtBot.Services;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Collections.Generic;

namespace DirtBot.Caching
{
    public class Cacher : ServiceBase
    {
        public static Dictionary<string, dynamic> DefaultKeys { get; } = new Dictionary<string, dynamic>()
        {
            { "greetingCount", 4 },
            { "maxGreetCount", 4 },
            { "RemoveAfter", 300 },
            { "Remove", true },
        };

        public Cacher(IServiceProvider services)
        {
            InitializeService(services);
            Client.MessageReceived += MessageRecievedAsync;
        }

        public async Task MessageRecievedAsync(SocketMessage message)
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
            if (Cache.CachedObjects.ContainsKey(guildChannel.Guild.Id.ToString())) 
            {
                // It has. Set the creation time again...
                Cache[message]["CreationTime"] = DateTime.Now;
            }
            else
            {
                Cache.CachedObjects.Add(guildChannel.Guild.Id.ToString(), new Dictionary<string, dynamic>());

                foreach (var key in DefaultKeys)
                {
                    Cache[message].Add(key.Key, key.Value);
                }
                
                // The time this has been created. Used for removing the cache after a while
                Cache[message].Add("CreationTime", DateTime.Now);
            }

            //CacheSave cacheSave = new CacheSave(socketGuildChannel.Guild.Id, socketGuildChannel.Guild.Name);
            //await Cache.AddToCacheAsync(cacheSave);
        }
    }
}
