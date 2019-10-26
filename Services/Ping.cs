using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DirtBot.Services
{
    public class Ping : ServiceBase
    {
        public Ping(IServiceProvider services)
        {
            InitializeService(services);
            discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task MessageReceivedAsync(SocketMessage arg)
        {
            if (IsSystemMessage(arg, out SocketUserMessage message)) return;

            if (message.Author.IsBot || message.Author == discord.CurrentUser) return;

            if (message.Content.ToLower().StartsWith("ping"))
            {
                await SendMessageIfAllowed("Pong!", message.Channel);
            }
        }
    }
}
