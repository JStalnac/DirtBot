using DirtBot.Database;
using DirtBot.Database.FileManagement;
using Discord;

namespace DirtBot.Helpers
{
    public static class GuildHelper
    {
        /*
         * Hello there future me! Did you remember to include the DirtBot.Helpers namespace?
         * That might be the problem if the extension method isn't showing.
         * I hope I saved myself some precious lifetime!
         */

        public static DataDirectory GetStorage(this IGuild guild)
        {
            ManagedDirectory guilds = FileManager.GetRegistedDirectory("Guilds");
            ManagedDirectory guildDirectory = guilds.GetDirectory(guild.Id.ToString());

            return new DataDirectory(guild.Id, guildDirectory, guild);
        }
    }
}
