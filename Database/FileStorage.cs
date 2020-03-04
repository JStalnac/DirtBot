using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using DirtBot.Commands;
using DirtBot.Database.FileManagement;
using DirtBot.Database.DatabaseObjects;
using System.Threading.Tasks;
using System.Reflection;

namespace DirtBot.Database
{
    public class FileStorage
    {
        static object locker = new object();
        static List<string> UsedFileNames { get; set; }
        public static List<IHasDataFile> FileDataModules { get; }

        public static Task LoadModuleStorage(Assembly assembly)
        {
            foreach (var module in FileDataModules)
            {
                if (UsedFileNames.Contains(module.FileName))
                {
                    throw new Exception($"A storage file with the name '{module.FileName}' exists already! Please choose a different name!");
                }

                UsedFileNames.Add(module.FileName);

                // TODO: Check if the file exist, create if it doesn't
                // TODO: Add things to FileDataModules
                // TODO: Make IHasDataFiles add the its data collections etc. to FileDataModules
            }

            return Task.CompletedTask;
        }

        public static void SetPrefix(IMessage message, string prefix)
        {
            if (message.Channel is SocketGuildChannel)
            {
                SetPrefix((message.Channel as SocketGuildChannel).Guild.Id, prefix);
            }
        }

        public static void SetPrefix(ulong id, string prefix)
        {
            lock (locker)
            {
                ManagedDirectory guilds = FileManager.GetRegistedDirectory("Guilds");
                ManagedDirectory guildDirectory = guilds.GetDirectory($"{id}");

                ManagedFile file = guildDirectory.GetFile("data.json");
                GuildDataObject guildData = file.ReadJsonData<GuildDataObject>() as GuildDataObject;
                
                guildData.Prefix = prefix;

                file.WriteJsonData(guildData);
            }

            new Caching.Cache()[id]["Prefix"] = prefix;
        }
    }
}
