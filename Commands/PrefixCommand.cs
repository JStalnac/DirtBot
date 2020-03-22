using DirtBot.Database;
using DirtBot.Helpers;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DirtBot.Commands
{
    public class PrefixCommand : ModuleBase<SocketCommandContext>, IHasDataFile
    {
        public string StaticStorage => "prefix.json";
        public string GuildStorage => "prefix.json";

        public List<ModuleData> DefaultData => new List<ModuleData>()
        {

        };

        public List<ModuleData> DefaultStaticData => new List<ModuleData>()
        {

        };

        public PrefixCommand()
        {
            
        }

        [Command("prefix")]
        [Alias("prefix")]
        [RequireContext(ContextType.Guild, ErrorMessage = "Et voi vaihtaa prefixiä yksityisviesteissä!")]
        public Task Prefix(string prefix)
        {
            // Guild channel
            if (prefix.Length > 12)
            {
                ReplyAsync("Prefix voi olla maksimissa vain 12 merkkiä pitkä!");
            }
            else
            {
                FileStorage.SetPrefix((Context.Channel as SocketGuildChannel).Guild.Id, prefix);
                ReplyAsync($"Palvelimesi uusi prefix on nyt {prefix}");
            }

            return Task.CompletedTask;
        }

        [Command("prefix")]
        [Alias("prefix")]
        public async Task Prefix()
        {
            string prefix = new Caching.Cache()[Context.Message]["Prefix"];
            ReplyAsync($"Prefixini on '{prefix}'");
        }
    }
}
