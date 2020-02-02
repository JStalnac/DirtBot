using System;
using System.Threading.Tasks;
using DirtBot.Caching;
using Discord.WebSocket;
using SmartFormat;

namespace DirtBot.Services
{
    public class Goodbye : ServiceBase
    {
        // These are things which people can say.
        string[] messages = { "moikka", "moikka", "heippa", "bye", "bye!" };
        // These will be capitalized. Stuff that I will respond.
        string[] responses = { "moikka!", "moikka! 👋", "Moikka {Username}! 👋", "Moikka {Username}!", "hyvää päivän jatkoa {Username}!", "👋" };

        public Goodbye(IServiceProvider services)
        {
            InitializeService(services);
            Client.MessageReceived += MessageRevievedAsync;
        }

        async Task MessageRevievedAsync(SocketMessage message)
        {
            if (message.Author.Id == Client.CurrentUser.Id) return;
            if (message.Source != Discord.MessageSource.User) return;
 
            foreach (string str in messages)
            {
                if (message.Content.ToLower().Contains(str))
                {
                    if (IsDMChannel(message.Channel)) 
                    {
                        string response = Capitalize(ChooseRandomString(responses));
                        await message.Channel.SendMessageAsync(string.Format(response, message.Author.Username));
                        return;
                    }

                    long greetingCount = Cache[message]["greetingCount"];
                    long maxGreetCount = Cache[message]["maxGreetCount"];

                    greetingCount++;
                    if (greetingCount >= maxGreetCount) 
                    {
                        string response = Capitalize(Smart.Format(ChooseRandomString(responses), message.Author));
                        await SendMessageIfAllowed(response, message.Channel);
                        greetingCount = 0;
                    }
                    await SendMessageIfAllowed($"Goodbye: greetingCount: {greetingCount}", message.Channel);

                    Cache[message]["greetingCount"] = greetingCount;

                    // Remember to return here!
                    return;
                }
            }
        }
    }
}
