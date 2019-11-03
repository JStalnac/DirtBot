﻿using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using SmartFormat;

namespace DirtBot.Services
{
    public class Mwhujöötrö : ServiceBase
    {
        string[] responses;

        public Mwhujöötrö(IServiceProvider services)
        {
            InitializeService(services);
            discord.MessageReceived += MessageRecievedAsync;

            string[] responses = { "Viimeksi kun söin mwhujöötröä se maistui ihan sokerimössöltä!", "Mwhujöötrö on pahaa!", "Onko mwhujöötrö susta hyvää {Username}?" };
            this.responses = responses;
        }

        public async Task MessageRecievedAsync(SocketMessage arg)
        {
            if (IsSystemMessage(arg, out SocketUserMessage message)) return;

            if (message.Content.Contains("Mwhujöötrö")) 
            {
                await SendMessageIfAllowed(Smart.Format(ChooseRandomString(responses), message.Author), message.Channel);
            }
        }
    }
}
