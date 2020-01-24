using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Discord.Rest;
using SmartFormat;

namespace DirtBot.Services
{
    public class DontPingMe : ServiceBase
    {
        // Stuff that the bot will respond with.
        string[] responses;

        public DontPingMe(IServiceProvider services)
        {
            InitializeService(services);
            Client.MessageReceived += MessageRevievedAsync;

            string[] responses = { "Älä tägää!!", "Älä tägää 😡", "Onko aina pakko tägätä?", "Ei oo kivaa! 😡", "Mur",
            "Miksi aina tägäät {Username}?", "Olisko kivaa jos mä tägäisin sut?", "{Mention}", "Lopeta! 😡",
            "Onko tämä kivaa? {Mention} {Mention}", "{Mention} {Mention} {Mention}", $"{Emojis["dirtdontpingme"]}" };
            this.responses = responses;
        }

        async Task MessageRevievedAsync(SocketMessage arg)
        {
            if (IsSystemMessage(arg, out SocketUserMessage message)) return;
            bool mentioned = false;

            foreach (ITag tag in message.Tags)
            {
                switch (tag.Type)
                {
                    case TagType.UserMention:
                        if (tag.Key == Client.CurrentUser.Id && !mentioned)
                        {
                            mentioned = true;
                            await SendAngryMessage(message);
                        }
                        break;

                    case TagType.EveryoneMention:
                        if (!mentioned) 
                        {
                            mentioned = true;
                            await SendAngryMessage(message);
                        }
                        break;

                    case TagType.HereMention:
                        goto case TagType.EveryoneMention;

                    default:
                        break;
                }
            }
        }

        private async Task SendAngryMessage(SocketUserMessage message)
        {
            // Send a funny message.
            string response = Capitalize(Smart.Format(ChooseRandomString(responses), message.Author));
            //await message.Channel.SendMessageAsync(response);
            await SendMessageIfAllowed(response, message.Channel);
            // And finish it of with a reaction.
            await AddReactionIfAllowed(Emojis["dirtdontpingme"], message);
        }
    }
}
