using System;
using System.IO;
using System.Text;
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
    internal class DataBasifier : ServiceBase
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
                guilds.AddFile($"{guild.Id}.json");
                ManagedFile file = guilds[$"{guild.Id}.json"];

                lock (locker)
                {
                    file.WriteJsonData(null);
                }
            }
        }
    }
}
