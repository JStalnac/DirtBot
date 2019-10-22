using System;
using System.Threading.Tasks;
using DirtBot.Caching;
using Discord.WebSocket;

namespace DirtBot.Services
{
    public class Greetings : ServiceBase
    {
        GreetingSharedDataObject cacheFallBackObject;

        // These are things which people can say.
        string[] messages = { "moi", "moi!", "terve", "terve!", "hei", "hei!", "huomenta!", "huomenta", "aamua", "aamua!", "iltaa", "iltaa!" };
        // These will be capitalized. Stuff that I will respond.
        string[] responses = { "moi", "moi! ", "moi 👋", "moi! 👋", "Moi {0}!", "Moi {0} 👋", "Moi {0}", "terve", "terve!", "terve 👋", "terve! 👋", "huomenta!", "huomenta", "oikein hyvää huomenta {0}",
        "hyvää huomenta {0}", "huomenta {0}", "huomenta {0} 👋" };

        public Greetings(IServiceProvider services)
        {
            InitializeService(services);
            cacheFallBackObject = new GreetingSharedDataObject("BYES");
            discord.MessageReceived += MessageRevievedAsync;
        }

        public async Task MessageRevievedAsync(SocketMessage arg)
        {
            if (ServiceHelper.IsSystemMessage(arg, out SocketUserMessage message)) return;
            if (message.Author.Id == discord.CurrentUser.Id) return;

            foreach (string str in messages)
            {
                if (message.Content.ToLower().TrimEnd(' ').Contains(str))
                {
                    if (ServiceHelper.IsDMChannel(message.Channel))
                    {
                        string response = ServiceHelper.Capitalize(ServiceHelper.ChooseRandomString(responses));
                        await message.Channel.SendMessageAsync(string.Format(response, message.Author.Username));
                    }

                    CacheSave cacheSave = await cache.GetFromCacheAsync(arg);
                    if (cacheSave is null) return;

                    GreetingSharedDataObject dataObject = await cacheSave.GetFromDataUnderKeyAsync("Greets", "BYES", cacheFallBackObject) as GreetingSharedDataObject;

                    dataObject.Value += 1;
                    if (dataObject.Value >= int.Parse(dataObject.DeafaultValue.ToString()))
                    {
                        string response = ServiceHelper.FormatMessage(ServiceHelper.ChooseRandomString(responses), message, true);
                        await ServiceHelper.SendMessageIfAllowed(response, message.Channel);
                        dataObject.Value = 0;
                    }
                }
            }
        }
    }
}
