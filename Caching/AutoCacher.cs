using System;
using DirtBot.Services;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DirtBot.Caching
{
    public class AutoCacher : ServiceBase
    {
        public AutoCacher(IServiceProvider services)
        {
            InitializeService(services);
            discord.MessageReceived += MessageRecievedAsync;
        }

        public async Task MessageRecievedAsync(SocketMessage arg)
        {
            // Filter system messages.
            SocketUserMessage message = arg as SocketUserMessage;
            if (message is null) return;
            // Filter DM messages.
            SocketGuildChannel socketGuildChannel = message.Channel as SocketGuildChannel;
            if (socketGuildChannel is null) return;

            // Don't cache bot messages. No user interaction there. If it is a command it has been triggered by a user.
            if (message.Author.IsBot) return;

            // By now we have only real user messages on guilds.

            // Add to cache
            CacheSave cacheSave = new CacheSave(socketGuildChannel.Guild.Id, socketGuildChannel.Guild.Name);
            await cache.AddToCacheAsync(cacheSave);
        }
    }
}
