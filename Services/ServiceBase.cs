using DirtBot.Caching;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DirtBot.Services
{
    /// <summary>
    /// The base service class.
    /// </summary>
    public class ServiceBase
    {
        protected CommandService Commands { get; private set; }
        protected DiscordSocketClient Client { get; private set; }
        protected IServiceProvider Services { get; private set; }
        protected Emojis Emojis { get; private set; }
        private Cache cache;

        protected Cache Cache { get => cache; }

        /// <summary>
        /// Call this in the constructor to initialize the service.
        /// </summary>
        /// <param name="services"></param>
        protected void InitializeService(IServiceProvider services)
        {
            Commands = services.GetRequiredService<CommandService>();
            Client = services.GetRequiredService<DiscordSocketClient>();
            cache = services.GetRequiredService<Cache>();
            Emojis = services.GetRequiredService<Emojis>();
            Services = services;

            // Adding the service.
            Commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        #region String utils
        /// <summary>
        /// Chooses a random string from an array or a list.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        protected string ChooseRandomString(string[] array)
        {
            return array[new Random().Next(0, array.Length)];
        }

        /// <summary>
        /// Chooses a random string from an array or a list.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        protected string ChooseRandomString(List<string> list)
        {
            return list[new Random().Next(0, list.Count)];
        }

        protected string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return char.ToUpper(value[0]) + value.Substring(1);
        }
        #endregion
    }
}
