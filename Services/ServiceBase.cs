using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using StackExchange.Redis;

namespace DirtBot.Services
{
    /// <summary>
    /// The base service class. Provides shortcuts to different services.
    /// </summary>
    public class ServiceBase
    {
        protected CommandService Commands => Services.GetRequiredService<CommandService>();
        protected DiscordSocketClient Client => Dirtbot.Client;
        protected static IServiceProvider Services => Dirtbot.Services;
        protected static ConnectionMultiplexer Redis => Dirtbot.Redis;
    }
}