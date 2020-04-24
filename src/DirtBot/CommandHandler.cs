using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace DirtBot
{
    public class CommandHandler
    {
        public static CommandsNextExtension CommandsNext { get; private set; }

        public CommandHandler(CommandsNextExtension commandsNext)
        {
            if (CommandsNext is null)
                CommandsNext = commandsNext;
        }

        public static async Task<int> GetPrefix(DiscordMessage msg)
        {
            // TODO: Get the prefix for the guild from Redis
            return CommandsNextUtilities.GetStringPrefixLength(msg, Config.Prefix);
        }
    }
}
