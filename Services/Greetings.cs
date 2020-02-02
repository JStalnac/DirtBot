using System;
using System.Threading.Tasks;
using Discord;
using SmartFormat;
using Discord.WebSocket;
using Dash.CMD;

namespace DirtBot.Services
{
    public class Greetings : ServiceBase
    {
        // These are things which people can say.
        string[] messages = { "moi", "moi!", "terve", "terve!", "hei", "hei!", "huomenta!", "huomenta", "aamua", "aamua!", "iltaa", "iltaa!" };
        string[] skippedMessages = { "moikka", "moikka", "heippa" };
        // These will be capitalized. Stuff that I will respond.
        string[] responses = { "moi", "moi! ", "moi 👋", "moi! 👋", "Moi {Username}!", "Moi {Username} 👋", "Moi {Username}", 
            "terve", "terve!", "terve 👋", "terve! 👋", "huomenta!", "huomenta", "oikein hyvää huomenta {Username}", 
            "hyvää huomenta {Username}", "huomenta {Username}", "huomenta {Username} 👋" };

        public Greetings(IServiceProvider services)
        {
            InitializeService(services);
            Client.MessageReceived += MessageRevievedAsync;
        }

        async Task MessageRevievedAsync(SocketMessage message)
        {
            bool currentUser = message.Author.Id == Client.CurrentUser.Id;
            bool notUserMessage = message.Source != MessageSource.User;

            DashCMD.WriteImportant($"Current user: {currentUser}, Not user message: {notUserMessage}");

            if (currentUser) return;
            if (notUserMessage) return;

            foreach (string str in messages)
            {
                foreach (string skipped in skippedMessages)
                {
                    if (message.Content.ToLower().TrimEnd(' ').Contains(str) && message.Content.ToLower().TrimEnd(' ').StartsWith(skipped))
                    {
                        if (IsDMChannel(message.Channel))
                        {
                            string response = Capitalize(ChooseRandomString(responses));
                            await message.Channel.SendMessageAsync(string.Format(response, message.Author.Username));
                        }
                        else
                        {
                            long greetingCount = Cache[message]["greetingCount"];
                            long maxGreetCount = Cache[message]["maxGreetCount"];

                            greetingCount++;
                            if (greetingCount >= maxGreetCount)
                            {
                                string response = Capitalize(Smart.Format(ChooseRandomString(responses), message.Author));
                                await SendMessageIfAllowed(response, message.Channel);
                                greetingCount = 0;
                            }
                            await SendMessageIfAllowed($"Greetings: greetingCount: {greetingCount}", message.Channel);

                            Cache[message]["greetingCount"] = greetingCount;
                        }
                        
                        // Remember to return!
                        return;
                    }
                }
            }
        }
    }
}
