﻿using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using DirtBot.Caching;

namespace DirtBot.Services
{
    public class FsInTheChat : ServiceBase
    {
        private FsInTheChatDataObject cacheFallBackObject;

        public FsInTheChat(IServiceProvider services) 
        {
            InitializeService(services);
            cacheFallBackObject = new FsInTheChatDataObject("F_COUNT");
            Client.MessageReceived += MessageReviecedAsync;
        }

        async Task MessageReviecedAsync(SocketMessage message)
        {
            if (message.Source != Discord.MessageSource.User) return;
            if (message.Author.Id == Client.CurrentUser.Id) return; // Don't respond to ourselves! That will make a bloooody mess!

            if (message.Content.ToLower() == "f" || message.Content.ToLower() == "f ")
            {
                if (IsDMChannel(message.Channel))
                { 
                    await message.Channel.SendMessageAsync("F");
                    return;
                }

                CacheSave cacheSave = await Cache.GetFromCacheAsync(message as SocketUserMessage);
                if (cacheSave is null) return;

                FsInTheChatDataObject obj = (await cacheSave.GetFromDataUnderKeyAsync("FsInTheChat", "F_COUNT", cacheFallBackObject)) as FsInTheChatDataObject;

                obj.Value += 1;
                if (obj.Value >= int.Parse(obj.DeafaultValue.ToString()))
                {
                    await SendMessageIfAllowed("F", message.Channel);
                    obj.Value = 0;
                }
            }
        }
    }

    class FsInTheChatDataObject : ICacheDataObject
    {
        private int defaultValue = 4;
        private string name;
        private int value;

        public FsInTheChatDataObject(string name)
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
