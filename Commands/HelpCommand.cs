using DirtBot.Attributes;
using DirtBot.Services;
using Discord.Commands;
using System.Threading.Tasks;

namespace DirtBot.Commands
{
    public class HelpCommand : ModuleBase<SocketCommandContext>
    {
        private readonly HelpProviderService help;

        public HelpCommand(HelpProviderService helpProvider)
        {
            help = helpProvider;
        }

        [Command("help")]
        [Tags("system")]
        public async Task Help([Remainder] string query)
        {
            var embed = await help.HelpAsync(Context, query, SearchContext.Commands);
            await ReplyAsync(embed: embed);
        }

        [Command("help-tag")]
        [Tags("system")]
        public async Task HelpTag([Remainder] string tag)
        {
            var embed = await help.HelpAsync(Context, tag, SearchContext.Tags);
            await ReplyAsync(embed: embed);
        }
    }
}
