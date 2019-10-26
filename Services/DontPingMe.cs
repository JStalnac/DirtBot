﻿using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using SmartFormat;

namespace DirtBot.Services
{
    public class DontPingMe : ServiceBase
    {
        // Stuff that the bot will respond with.
        string[] responses = { "Älä tägää!!", "Älä tägää 😡", "Onko aina pakko tägätä?", "Ei oo kivaa! 😡", "Mur",
            "Miksi aina tägäät {Username}?", "Olisko kivaa jos mä tägäisin sut?", "{Mention}", "Lopeta! 😡", "Onko tämä kivaa? {Mention}", "{Mention} {Mention} {Mention}" };

        public DontPingMe(IServiceProvider services)
        {
            InitializeService(services);
            discord.MessageReceived += MessageRevievedAsync;
        }

        public async Task MessageRevievedAsync(SocketMessage arg)
        {
            if (IsSystemMessage(arg, out SocketUserMessage message)) return;
            bool mentioned = false;

            foreach (ITag tag in message.Tags)
            {
                switch (tag.Type)
                {
                    case TagType.UserMention:
                        if (tag.Key == discord.CurrentUser.Id && !mentioned)
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
            await ServiceHelper.SendMessageIfAllowed(response, message.Channel);
            await ServiceHelper.AddReactionIfAllowed(emojis.DirtDontPingMe, message);
        }
    }
}