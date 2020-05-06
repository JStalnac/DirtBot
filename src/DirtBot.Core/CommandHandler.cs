using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace DirtBot.Core
{
    public class CommandHandler
    {
        readonly IServiceProvider services;

        public CommandHandler(IServiceProvider services)
        {
            this.services = services;
        }

        public static async Task<int> GetPrefix(DiscordMessage msg)
        {
            // TODO: Get the prefix for the guild from Redis
            return CommandsNextUtilities.GetStringPrefixLength(msg, Config.Prefix);
        }
    }
}
