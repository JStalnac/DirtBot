using DirtBot.Modules;
using DirtBot.Services;
using Discord.Commands;
using System.Threading.Tasks;
using DirtBot.Attributes;

namespace DirtBot.Commands
{
    public class PrefixCommand : Module
    {
        public override string DisplayName => "Prefix";

        [Command("set_prefix")]
        [Alias("prefix")]
        [RequireContext(ContextType.Guild, ErrorMessage = "Et voi vaihtaa prefixiä yksityisviesteissä!")]
        public Task Prefix(string prefix, [Remainder]string args = null)
        {
            Logging.Logger.Log("PrefixCommand :pog:");

            // Guild channel
            if (prefix.Length > 12)
            {
                ReplyAsync("Prefix voi olla maksimissa vain 12 merkkiä pitkä!");
            }
            else
            {
                ReplyAsync($"Palvelimenne prefix ei ole nyt **'{prefix}'**.");
            }

            if (!(args is null))
            {
                ReplyAsync($"Vinkki: Voit laittaa \"-merkin tekstin molemmille puolille, jolloin saat prefixiisi välejä.```\"Tällä tavalla :o\"```");
            }
            return Task.CompletedTask;
        }

        [Command("get_prefix")]
        [Alias("prefix")]
        public async Task Prefix()
        {
            string prefix = Config.Prefix;
            if (!Context.IsPrivate)
                prefix = CommandHandlingService.GetPrefix(Context.Guild.Id);

            await ReplyAsync($"Prefixini on **'{prefix}'**");
            if (Context.IsPrivate)
                ReplyAsync("Olemme yksityisviesteissä, joten et voi vaihtaa prefixiä.");
        }
    }
}
