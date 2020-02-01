using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using DirtBot.DataBase;
using DirtBot.DataBase.FileManagement;
using DirtBot.DataBase.DataBaseObjects;

namespace DirtBot.Modules
{
    public class SetPrefix : ModuleBase<SocketCommandContext>
    {
        object locker = new object();

        // In the future make the prefix cached

        [Command("prefix")]
        public Task Prefix([Summary("New prefix")] string prefix) 
        {
            // Filter
            if (Context.Channel is SocketDMChannel) 
            {
                ReplyAsync("Anteeksi. Et voi vaihtaa prefixiä yksityisviesteissä.");
                return Task.CompletedTask;
            }

            if (prefix.Length > 12) 
            {
                ReplyAsync("Olen pahoillani. Antamasi prefix on liian pitkä. Maksimi pituus on 12 merkkiä.");
                return Task.CompletedTask;
            }
            // TODO: Check if the prefix is the same as the current one...

            ManagedDirectory guilds = FileManager.GetRegistedDirectory("Guilds");
            SocketGuildChannel guildChannel = Context.Channel as SocketGuildChannel;
            ManagedFile file = guilds.GetDirectory(guildChannel.Guild.Id.ToString()).GetFile("data.json");

            lock (locker)
            {
                GuildDataObject currentGuild = file.ReadJsonData<GuildDataObject>() as GuildDataObject;
                currentGuild.Prefix = prefix;

                file.WriteJsonData(currentGuild, Newtonsoft.Json.Formatting.Indented);
            }

            ReplyAsync($"Palvelimen uusi prefix on nyt **{prefix}**");

            return Task.CompletedTask;
        }
    }
}
