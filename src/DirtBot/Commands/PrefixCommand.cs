using DirtBot.Services;
using Discord.Commands;
using System.Threading.Tasks;
using DirtBot.Helpers;
using DirtBot.Database;
using System;
using DirtBot.Logging;

namespace DirtBot.Commands
{
    public class PrefixCommand : ModuleBase<SocketCommandContext>
    {
        [Command("set_prefix")]
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
                // C# Binary formatters will handle all other characters (hopefully). No need to escape anything
                var storage = Context.Guild.GetStorageFile("prefix.bin");
                // Set the new prefix
                var data = new DataCollectionBuilder<string, string>()
                    .Add("prefix", prefix)
                    .BuildReadOnlyDataCollection();
                try
                {
                    storage.WriteAsBinary(data);
                    CommandHandlingService.prefixCache.Set(Context.Guild.Id, prefix);
                }
                catch (Exception e)
                {
                    Logger.Log($"Failed to set prefix for guild {Context.Guild.Id}!", true, e, fore: ConsoleColor.Yellow);
                    ReplyAsync($"Tapahtui virhe. Ilmoitathan tästä pomolleni.");
                    return Task.CompletedTask;
                }
                ReplyAsync($"Palvelimenne prefix on nyt **'{prefix}'**.");
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
