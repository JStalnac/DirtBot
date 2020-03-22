using DirtBot.Commands;
using DirtBot.Database.FileManagement;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DirtBot.Database
{
    public class JsonFileContent
    {
        public ManagedFile File { get; }
        public List<ModuleData> Data { get; private set; }
        public bool IsValidJson { get; private set; }

        public JsonFileContent(ManagedFile file)
        {
            File = file;
            Reload();
        }

        public ModuleData? Get(string name)
        {
            foreach (var data in Data)
            {
                if (data.Name == name)
                {
                    return data;
                }
            }
            return null;
        }

        public void Set(string name, dynamic value)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i].Name == name)
                {
                    Data[i] = new ModuleData(name, value, Data[i].DocString);
                    return;
                }
            }
        }

        public void Write(int times = 100)
        {
            for (int i = 0; i < times; i++)
            {
                if (File.TryAcquireLock())
                {
                    try
                    {
                        File.WriteAllText(JsonConvert.SerializeObject(Data));
                    }
                    finally
                    {
                        File.ReleaseLock();
                    }

                    return;
                }
                else
                {
                    Thread.Sleep(50);
                }
            }

            throw new IOException($"Failed to write data to file '{File.FileInfo.FullName}'");
        }
        public Task WriteAsync(int times = 100)
        {
            Write(times);
            return Task.CompletedTask;
        }

        public void Reload(int times = 100)
        {
            for (int i = 0; i < times; i++)
            {
                if (File.TryAcquireLock())
                {
                    try
                    {
                        string contents = File.ReadAllText();
                        Data = JsonConvert.DeserializeObject<List<ModuleData>>(contents);
                        IsValidJson = true;
                    }
                    catch (JsonException)
                    {
                        IsValidJson = false;
                    }
                    finally
                    {
                        File.ReleaseLock();
                    }

                    return;
                }
                else
                {
                    Thread.Sleep(50);
                }
            }

            throw new IOException($"Failed to read data from file '{File.FileInfo.FullName}'");
        }
        public Task ReloadAsync(int times = 100)
        {
            Reload(times);
            return Task.CompletedTask;
        }
    }
}
