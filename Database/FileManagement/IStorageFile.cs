using DirtBot.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DirtBot.Database.FileManagement
{
    public interface IStorageFile
    {
        string FileName { get; }
        ManagedFile ManagedFile { get; set; }
        List<ModuleData> Data { get; }

        Task Load();
        Task SetValue(string name, dynamic value);
        Task GetValue(string name);
    }
}
