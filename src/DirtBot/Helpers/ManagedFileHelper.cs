using DirtBot.Database.FileManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace DirtBot.Helpers
{
    public static class ManagedFileHelper
    {
        public static void WriteAsBinary(this ManagedFile file, object obj)
        {
            for (int i = 0; i < 71; i++)
            {
                if (file.TryAcquireLock())
                {
                    try
                    {
                        var fs = file.OpenWrite();
                        var bf = new BinaryFormatter();
                        bf.Serialize(fs, obj);
                        fs.Close();
                    }
                    finally
                    {
                        file.ReleaseLock();
                    }
                    return;
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
            throw new IOException($"Could not write to file {file.FileName}");
        }

        public static T ReadAsBinary<T>(this ManagedFile file)
        {
            for (int i = 0; i < 51; i++)
            {
                if (file.TryAcquireLock())
                {
                    object obj;
                    try
                    {
                        var fs = file.OpenRead();
                        var bf = new BinaryFormatter();
                        obj = bf.Deserialize(fs);
                    }
                    finally
                    {
                        file.ReleaseLock();
                    }
                    return (T)obj;
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
            throw new IOException($"Could not read file {file.FileName}");
        }
    }
}
