using DirtBot.Caching;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace DirtBot.Services
{
    /// <summary>
    /// The base service class.
    /// </summary>
    public class ServiceBase
    {
        protected CommandService Commands { get; set; }
        protected DiscordSocketClient Client { get; set; }
        protected static IServiceProvider Services { get; set; }
        protected Emojis Emojis { get; set; }
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
    }
}
