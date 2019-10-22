using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DirtBot.Caching;

namespace DirtBot.Services
{
    /// <summary>
    /// The base service class.
    /// </summary>
    public class ServiceBase
    {
        protected CommandService commands;
        protected DiscordSocketClient discord;
        protected Config config;
        protected IServiceProvider services;
        protected Cache cache;
        protected Emojis emojis;

        /// <summary>
        /// Call this in the constructor to initialize the service.
        /// </summary>
        /// <param name="services"></param>
        protected void InitializeService(IServiceProvider services)
        {
            commands = services.GetRequiredService<CommandService>();
            discord = services.GetRequiredService<DiscordSocketClient>();
            config = services.GetRequiredService<Config>();
            cache = services.GetRequiredService<Cache>();
            emojis = services.GetRequiredService<Emojis>();
            this.services = services;

            // Adding the service.
            commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }
    }
}
