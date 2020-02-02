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
        string[] messages = { "moi", "terve", "hei", "huomenta", "aamua", "iltaa" };
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
            // No responding to ourselves or to the system or to our bot bros!
            if (message.Author.Id == Client.CurrentUser.Id) return;
            if (message.Source != MessageSource.User) return;

            foreach (string str in messages)
            {
                foreach (string skipped in skippedMessages)
                {
                    if (!message.Content.ToLower().Contains(skipped))
                    {
                        continue;
                    }
                    else
                    {
                        // They are saying goodbye
                        return;
                    }
                }

                // Message filtered
                if (message.Content.ToLower().Contains(str)) 
                {
                    if (IsDMChannel(message.Channel))
                    {
                        string response = Capitalize(ChooseRandomString(responses));
                        await message.Channel.SendMessageAsync(Smart.Format(response, message.Author));
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
