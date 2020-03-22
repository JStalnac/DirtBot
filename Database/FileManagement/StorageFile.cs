using DirtBot.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DirtBot.Database.FileManagement
{
    public abstract class StorageFile : IStorageFile
    {
        public abstract string FileName { get; }

        public ManagedFile ManagedFile { get; set; }

        public List<ModuleData> Data { get; } = new List<ModuleData>();

        public Task GetValue(string name)
        {
            throw new NotImplementedException();
        }

        public Task Load()
        {
            throw new NotImplementedException();
        }

        public Task SetValue(string name, dynamic value)
        {
            throw new NotImplementedException();
        }
    }
}
