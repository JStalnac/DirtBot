using System;
using System.Threading.Tasks;
using DirtBot.Caching;
using Discord.WebSocket;
using SmartFormat;

namespace DirtBot.Services
{
    public class Goodbye : ServiceBase
    {
        GreetingSharedDataObject cacheFallBackObject;

        // These are things which people can say.
        string[] messages = { "moikka", "moikka", "heippa", "heippa", "bye", "bye!" };
        // These will be capitalized. Stuff that I will respond.
        string[] responses = { "moikka!", "moikka! 👋", "Moikka {Username}! 👋", "Moikka {Username}!", "hyvää päivän jatkoa {Username}!", "👋" };

        public Goodbye(IServiceProvider services)
        {
            InitializeService(services);
            cacheFallBackObject = new GreetingSharedDataObject("BYES");
            discord.MessageReceived += MessageRevievedAsync;
        }

        public async Task MessageRevievedAsync(SocketMessage arg)
        {
            if (IsSystemMessage(arg, out SocketUserMessage message)) return;
            if (message.Author.Id == discord.CurrentUser.Id) return;

            foreach (string str in messages)
            {
                if (message.Content.ToLower().Contains(str))
                {
                    if (IsDMChannel(message.Channel)) 
                    {
                        string response = Capitalize(ChooseRandomString(responses));
                        await message.Channel.SendMessageAsync(string.Format(response, message.Author.Username));
                    }

                    CacheSave cacheSave = await cache.GetFromCacheAsync(arg);
                    if (cacheSave is null) return;

                    GreetingSharedDataObject dataObject = await cacheSave.GetFromDataUnderKeyAsync("Greets", "BYES", cacheFallBackObject) as GreetingSharedDataObject;

                    dataObject.Value += 1;
                    if (dataObject.Value >= int.Parse(dataObject.DeafaultValue.ToString())) 
                    {
                        string response = Capitalize(Smart.Format(ChooseRandomString(responses), message.Author));
                        //string response = ServiceHelper.FormatMessage(ChooseRandomString(responses), message, true);
                        await ServiceHelper.SendMessageIfAllowed(response, message.Channel);
                        dataObject.Value = 0;
                    }
                }
            }
        }
    }

    class GreetingSharedDataObject : ICacheDataObject
    {
        protected int defaultValue = 4;
        protected string name;
        protected int value;

        public GreetingSharedDataObject(string name)
        {
            this.name = name;
            value = defaultValue;
        }

        public object DeafaultValue 
        {
            get { return defaultValue; }
        }

        public int Value 
        {
            get { return value; }
            set { this.value = value; }
        }

        public string Name 
        {
            get { return name; }
            set { name = value; }
        }
    }
}
