using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.Commands;
using Discord.WebSocket;
using DirtBot.Database;
using DirtBot.Database.FileManagement;
using DirtBot.Database.DatabaseObjects;

namespace DirtBot.Commands
{
    public class PrefixCommand : ModuleBase<SocketCommandContext>, IHasDataFile
    {
        public string FileName { get; } = "prefix.json";

        public CommandData[] Data { get; }
        public IReadOnlyCollection<CommandData> DataFormat { get; }

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
