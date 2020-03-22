using DirtBot.Caching;
using DirtBot.Database.DatabaseObjects;
using DirtBot.Database.FileManagement;
using DirtBot.Services;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DirtBot.Database
{
    /// <summary>
    /// Adds guilds to our database if they're not in yet.
    /// </summary>
    public class DataBasifier : ServiceBase
    {
        static object locker = new object();

        public DataBasifier(IServiceProvider services)
        {
            InitializeService(services);
            Client.MessageReceived += MessageReceivedAsync;
        }

        private async Task MessageReceivedAsync(SocketMessage arg)
        {
            await CreateGuildAsync(arg);
        }

        public static async Task CreateGuildAsync(SocketMessage message)
        {
            if (message.Channel is SocketDMChannel) return;

            SocketGuild guild = (message.Channel as SocketGuildChannel).Guild;
            ManagedDirectory guilds = FileManager.GetRegistedDirectory("Guilds");

            try
            {
                ManagedDirectory managed = guilds.GetDirectory($"{guild.Id}");
                if (managed.Files is null)
                {
                    throw new FileNotFoundException();
                }
            }
            catch (FileNotFoundException)
            {
                // No guild stored, create it...
                Logger.Log($"Creating file for guild '{guild.Name}' ({guild.Id})...", true, foregroundColor: ConsoleColor.White);

                lock (locker)
                {
                    guilds.CreateSubdirectory(guild.Id.ToString());
                    ManagedDirectory guildDirectory = guilds.GetDirectory($"guilds/{guild.Id}");

                    guildDirectory.AddFile("data.json");
                    guildDirectory.GetFile("data.json").WriteAllText(
                        JsonConvert.SerializeObject(new GuildDataObject(guild.Id, Config.Prefix), Formatting.Indented));
                }

                Logger.Log($"Finished creating file for guild '{guild.Name}' ({guild.Id})!", true, foregroundColor: ConsoleColor.White);
            }
            finally
            {
                // Caching the guild
                ManagedDirectory guildDirectory = guilds.GetDirectory(guild.Id.ToString());

                ManagedFile file = guildDirectory.GetFile("data.json");

                GuildDataObject guildData = JsonConvert.DeserializeObject<GuildDataObject>(file.ReadAllText());

                // Adding the values to cache
                Cache cache = new Cache();
                cache[message]["Prefix"] = guildData.Prefix;
            }
        }
    }
}
