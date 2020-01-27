using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using DirtBot.DataBase;
using DirtBot.DataBase.FileManagement;

namespace DirtBot.Modules
{
    public class SetPrefix : ModuleBase<SocketCommandContext>
    {
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

            ManagedDirectory guilds = FileManager.GetRegistedDirectory("Guilds");
            ManagedFile file = guilds.GetFile((Context.Channel as SocketGuildChannel).Guild.Id.ToString());

            GuildDataBaseObject currentGuild = file.ReadJsonData<GuildDataBaseObject>() as GuildDataBaseObject;
            currentGuild.Prefix = prefix;

            file.WriteJsonData(currentGuild, Newtonsoft.Json.Formatting.Indented);

            ReplyAsync($"Palvelimen uusi prefix on nyt **{prefix}**");

            return Task.CompletedTask;
        }
    }
}
