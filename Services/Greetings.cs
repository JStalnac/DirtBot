using System;
using System.Threading.Tasks;
using DirtBot.Caching;
using Discord.WebSocket;
using SmartFormat;

namespace DirtBot.Services
{
    public class Greetings : ServiceBase
    {
        GreetingSharedDataObject cacheFallBackObject;

        // These are things which people can say.
        string[] messages = { "moi", "moi!", "terve", "terve!", "hei", "hei!", "huomenta!", "huomenta", "aamua", "aamua!", "iltaa", "iltaa!" };
        // These will be capitalized. Stuff that I will respond.
        string[] responses = { "moi", "moi! ", "moi 👋", "moi! 👋", "Moi {Username}!", "Moi {Username} 👋", "Moi {Username}", 
            "terve", "terve!", "terve 👋", "terve! 👋", "huomenta!", "huomenta", "oikein hyvää huomenta {Username}", 
            "hyvää huomenta {Username}", "huomenta {Username}", "huomenta {Username} 👋" };

        public Greetings(IServiceProvider services)
        {
            InitializeService(services);
            cacheFallBackObject = new GreetingSharedDataObject("BYES");
            Discord.MessageReceived += MessageRevievedAsync;
        }

        async Task MessageRevievedAsync(SocketMessage arg)
        {
            if (IsSystemMessage(arg, out SocketUserMessage message)) return;
            if (message.Author.Id == Discord.CurrentUser.Id) return;

            foreach (string str in messages)
            {
                if (message.Content.ToLower().TrimEnd(' ').Contains(str))
                {
                    if (IsDMChannel(message.Channel))
                    {
                        string response = Capitalize(ChooseRandomString(responses));
                        await message.Channel.SendMessageAsync(string.Format(response, message.Author.Username));
                    }

                    CacheSave cacheSave = await Cache.GetFromCacheAsync(arg);
                    if (cacheSave is null) return;

                    GreetingSharedDataObject dataObject = await cacheSave.GetFromDataUnderKeyAsync("Greets", "BYES", cacheFallBackObject) as GreetingSharedDataObject;

                    dataObject.Value += 1;
                    if (dataObject.Value >= int.Parse(dataObject.DeafaultValue.ToString()))
                    {
                        string response = Capitalize(Smart.Format(ChooseRandomString(responses), message.Author));
                        await SendMessageIfAllowed(response, message.Channel);
                        dataObject.Value = 0;
                    }
                }
            }
        }
    }
}
