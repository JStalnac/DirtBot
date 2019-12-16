using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DirtBot.Services
{
    public class Scares : ServiceBase
    {
        string[] scares = { "Hui", "Hui!", "Huih", "Huih!", "Huiuiui", "Huui", "Huui!", };
        string[] scaresCutOf = { "Hu-", "Hu", "Hu-...", "Hu...", };
        string[] notScary = { "Ei pelota!", "Ei oo pelottava", "Hyvä yritys, en noin helposta säiky :)", "Ei pelota yhtään! Tai ehkä vähäsen..." };

        public Scares(IServiceProvider services)
        {
            InitializeService(services);
            Discord.MessageReceived += MessageRecievedAsync;
        }

        async Task MessageRecievedAsync(SocketMessage arg)
        {
            // Filter system messages and stop executing if the author is this bot.
            if (IsBotMessage(arg, out SocketUserMessage message)) return;

            string content = message.Content;

            // Normal scares
            if (content.ToLower().StartsWith("böö"))
                await SendMessageIfAllowed(ChooseRandomString(scares), message.Channel);
            // Scares that are cut of cuz why not lol
            else if (content.ToLower().StartsWith("bö-") || content.ToLower().StartsWith("bö-...") || content.ToLower().StartsWith("bö...") || content.ToLower().StartsWith("bö"))
                await SendMessageIfAllowed(ChooseRandomString(scaresCutOf), message.Channel);

            else if (content.ToLower().StartsWith("pöö"))
                await SendMessageIfAllowed(ChooseRandomString(notScary), message.Channel);
        }
    }
}
