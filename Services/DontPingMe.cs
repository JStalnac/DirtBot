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
        string[] cantUseEmojis = { "En saa käyttää omia emojeja :c", "Mä haluan käyttää omia emojejani 😭", "Antakaa mun käyttää omia emojeja!", 
            "Mä en saa käyttää omia emojeja. :c Älä silti tägää! 😡" };

        public DontPingMe(IServiceProvider services)
        {
            InitializeService(services);
            Discord.MessageReceived += MessageRevievedAsync;

            string[] responses = { "Älä tägää!!", "Älä tägää 😡", "Onko aina pakko tägätä?", "Ei oo kivaa! 😡", "Mur",
            "Miksi aina tägäät {Username}?", "Olisko kivaa jos mä tägäisin sut?", "{Mention}", "Lopeta! 😡",
            "Onko tämä kivaa? {Mention} {Mention}", "{Mention} {Mention} {Mention}", $"{Emojis.DirtDontPingMe}" };
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
                        if (tag.Key == Discord.CurrentUser.Id && !mentioned)
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
            string response = Capitalize(Smart.Format(ChooseRandomString(responses), message.Author));

            try
            {
                RestUserMessage restMessage = await message.Channel.SendMessageAsync(response);

                if (!restMessage.Content.Contains("<:" + Emojis.DirtDontPingMe.Name + ":"))
                {
                    await SendMessageIfAllowed(ChooseRandomString(cantUseEmojis), message.Channel);
                }
            }
            catch (Discord.Net.HttpException e)
            {
                // Some Discord or permission error happened

                // Cannot send messages, doesn't matter
                if (e.DiscordCode == 50013) { }
            }
            await AddReactionIfAllowed(Emojis.DirtDontPingMe, message);
        }
    }
}
