using DirtBot.Services;
using Discord.Commands;
using System.Threading.Tasks;

namespace DirtBot.Commands
{
    public class PrefixCommand : ModuleBase<SocketCommandContext>
    {
        [Command("prefix")]
        [Alias("prefix")]
        [RequireContext(ContextType.Guild, ErrorMessage = "Et voi vaihtaa prefixiä yksityisviesteissä!")]
        public Task Prefix(string prefix, [Remainder]string args = null)
        {
            // Guild channel
            if (prefix.Length > 12)
            {
                ReplyAsync("Prefix voi olla maksimissa vain 12 merkkiä pitkä!");
            }
            else
            {
                CommandHandlingService.prefixCache.Set(Context.Guild.Id, prefix);
                ReplyAsync($"Palvelimenne prefix on nyt **'{prefix}'**.");
            }

            if (!(args is null))
            {
                ReplyAsync($"Vinkki: Voit laittaa \"-merkin tekstin molemmille puolille, jolloin saat prefixiisi välejä.```\"Tällä tavalla :o\"```");
            }
            return Task.CompletedTask;
        }

        [Command("prefix", ignoreExtraArgs: true)]
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
