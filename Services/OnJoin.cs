using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DirtBot.Services
{
    class OnJoin : ServiceBase
    {
        public OnJoin(IServiceProvider services)
        {
            InitializeService(services);
            discord.GuildAvailable += GuildAvailableAsync;
        }

        public async Task GuildAvailableAsync(SocketGuild arg)
        {
            Console.WriteLine($"Guild available: {arg.Name} ({arg.Id})");
        }
    }
}
