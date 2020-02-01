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
            SocketGuildChannel socketGuildChannel = message.Channel as SocketGuildChannel;

            // Add to cache
            CacheSave cacheSave = new CacheSave(socketGuildChannel.Guild.Id, socketGuildChannel.Guild.Name);
            await Cache.AddToCacheAsync(cacheSave);
        }
    }
}
