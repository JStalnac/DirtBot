using DirtBot.Logging;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DirtBot.Services
{
    class OnJoin : ServiceBase
    {
        public OnJoin(IServiceProvider services)
        {
            InitializeService(services);
            Client.GuildAvailable += GuildAvailableAsync;
        }

        async Task GuildAvailableAsync(SocketGuild arg)
        {
            Logger.Log($"Guild available: {arg.Name} ({arg.Id})", true, foregroundColor: ConsoleColor.White);
        }
    }
}
