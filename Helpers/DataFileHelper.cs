using DirtBot.Commands;
using DirtBot.Database;
using DirtBot.Database.FileManagement;
using Newtonsoft.Json;
using System.IO;

namespace DirtBot.Helpers
{
    public static class DataFileHelper
    {
        public static JsonFileContent GetStaticStorage(this IHasDataFile dataFile, string module)
        {
            return new JsonFileContent(FileManager.GetRegistedDirectory("Commands").GetFile(module));
        }

        public static void CreateStaticStorage(this IHasDataFile dataFile)
        {
            var storage = FileManager.GetRegistedDirectory("Commands");
            if (storage.GetFile(dataFile.StaticStorage) is null)
            {
                // There is no file with this name.
                storage.AddFile(dataFile.StaticStorage);
            }
            else
                throw new IOException($"A static storage file with the name '{dataFile.StaticStorage}' already exists. Please choose a diffrent name");
        }

        public static bool EnsureStaticStorage(this IHasDataFile dataFile)
        {
            var storage = FileManager.GetRegistedDirectory("Commands");
            if (storage.GetFile(dataFile.StaticStorage) is null)
            {
                // There is no file with this name.
                storage.AddFile(dataFile.StaticStorage);
            }
            else
                return false;
            return true;
        }

        public static JsonFileContent GetStorage(this IHasDataFile dataFile, ulong guildId)
        {
            var guild = FileManager.GetRegistedDirectory("Guilds").GetDirectory(guildId.ToString());
            if (!(guild is null))
            {
                var file = guild.GetFile(dataFile.GuildStorage);
                if (!(file is null))
                {
                    return new JsonFileContent(file);
                }
            }
            return null;
        }

        public static void CreateStorage(this IHasDataFile dataFile, ulong guild)
        {
            var storage = FileManager.LoadDirectory("Guilds").GetDirectory(guild.ToString());
            if (!(storage is null))
            {
                // We have storage
                var file = storage.GetFile(dataFile.GuildStorage);

                if (!(file is null))
                    // There is a file with this name...
                    throw new IOException($"A storage file with the name '{dataFile.GuildStorage}' already exists. Please choose a different name.");

                file = storage.AddFile(dataFile.GuildStorage);
                file.WriteAllText(JsonConvert.SerializeObject(dataFile.DefaultData));
            }
        }

        public static bool EnsureStorage(this IHasDataFile dataFile, ulong guild)
        {
            var storage = FileManager.LoadDirectory("Guilds").GetDirectory(guild.ToString());
            if (!(storage is null))
            {
                // We have storage
                var file = storage.GetFile(dataFile.GuildStorage);
                
                if (!(file is null))
                    // There is a file with this name...
                    return false;

                file = storage.AddFile(dataFile.GuildStorage);
                file.WriteAllText(JsonConvert.SerializeObject(dataFile.DefaultData));
            }
            return true;
        }
    }
}
