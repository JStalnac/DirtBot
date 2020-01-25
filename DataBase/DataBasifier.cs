using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.WebSocket;
using DirtBot.Services;
using DirtBot.DataBase.FileManagement;

namespace DirtBot.DataBase
{
    /// <summary>
    /// Adds guilds to our database if they're not in yet.
    /// </summary>
    public class DataBasifier : ServiceBase
    {
        object locker = new object();

        public DataBasifier(IServiceProvider services)
        {
            InitializeService(services);

            Client.MessageReceived += MessageReceivedAsync;
        }

        private async Task MessageReceivedAsync(SocketMessage arg)
        {
            SocketGuild guild = (arg.Channel as SocketGuildChannel).Guild;
            ManagedDirectory guilds = FileManager.GetRegistedDirectory("Guilds");
            try
            {
                guilds.GetFile($"{guild.Id}.json");
            }
            catch (FileNotFoundException)
            {
                // No guild stored, create it...
                await Logger.InfoAsync($"Creating file for guild '{guild.Name}' ({guild.Id})...");

                lock (locker)
                {
                    guilds.AddFile($"{guild.Id}.json");
                    ManagedFile file = guilds[$"{guild.Id}.json"];

                    file.WriteJsonData(new GuildDataBaseObject(guild.Id, guild.Name, Config.Prefix), Newtonsoft.Json.Formatting.Indented);
                }
                await Logger.InfoAsync($"Finished creating file for guild '{guild.Name}' ({guild.Id})!");
            }
        }
    }
}
