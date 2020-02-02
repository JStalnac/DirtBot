using System;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using DirtBot.Services;
using DirtBot.DataBase.FileManagement;
using DirtBot.DataBase.DataBaseObjects;
using Dash.CMD;

namespace DirtBot.DataBase
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
                DashCMD.WriteStandard($"Creating file for guild '{guild.Name}' ({guild.Id})...");

                lock (locker)
                {
                    guilds.CreateSubdirectory(guild.Id.ToString());
                    ManagedDirectory guildDirectory = guilds.GetDirectory($"guilds/{guild.Id}");

                    guildDirectory.AddFile("data.json");
                    guildDirectory["data.json"].WriteJsonData(new GuildDataObject(guild.Id, Config.Prefix), Newtonsoft.Json.Formatting.Indented);
                }

                DashCMD.WriteStandard($"Finished creating file for guild '{guild.Name}' ({guild.Id})!");
            }
        }
    }
}
