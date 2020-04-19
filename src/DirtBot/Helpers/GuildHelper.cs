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

        /// <summary>
        /// Gets the storage directory for this guild. Creates it if it doesn't exist.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public static ManagedDirectory GetStorage(this IGuild guild)
        {
            var guilds = FileManager.GetRegistedDirectory("Guilds");
            return guilds.CreateSubdirectory(guild.Id.ToString());
        }

        /// <summary>
        /// Gets a storage file by name from this guild's storage directory. Creates it if it doesn't exist.
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ManagedFile GetStorageFile(this IGuild guild, string name)
        {
            var s = GetStorage(guild);
            if (s.GetFile(name) == null)
            {
                s.CreateFile(name);
            }
            return s.GetFile(name);
        }
    }
}