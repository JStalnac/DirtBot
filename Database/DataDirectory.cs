using DirtBot.Database.FileManagement;
using Discord;
using System.Collections.Generic;

namespace DirtBot.Database
{
    public class DataDirectory
    {
        public ulong? Id { get; }
        public ManagedDirectory Storage { get; }
        public IGuild Guild { get; }
        public bool IsGuildStorage { get => Id.HasValue; }

        public Dictionary<string, JsonFileContent> Data { get; } = new Dictionary<string, JsonFileContent>();

        public DataDirectory(ulong? id, ManagedDirectory storage, IGuild guild)
        {
            if (id == 0)
                id = null;

            Id = id;
            Storage = storage;
            Guild = guild;

            foreach (var file in Storage.Files)
            {
                Data.Add(file.FileInfo.Name, new JsonFileContent(file));
            }
        }

        public void ReloadAllFiles()
        {
            foreach (var file in Data.Values)
            {
                file.Reload();
            }
        }

        public void WriteAllFiles()
        {
            foreach (var file in Data.Values)
            {
                file.Reload();
            }
        }
    }
}
