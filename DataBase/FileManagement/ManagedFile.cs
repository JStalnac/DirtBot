using System;
using System.IO;
using Newtonsoft.Json;

namespace DirtBot.Database.FileManagement
{
    public struct ManagedFile : IDisposable
    {
        private object locker;
        private FileInfo FileInfo;
        public string Name { get => FileInfo.Name; }
        public string FullName { get => FileInfo.FullName; }
        public string Extension { get => FileInfo.Extension; }
        public long Lenght { get => FileInfo.Length; }
        public DirectoryInfo Directory { get => FileInfo.Directory; }
        public string DirectoryName { get => FileInfo.DirectoryName; }
        public bool IsReadOnly { get => FileInfo.IsReadOnly; }
        public bool Exists { get => FileInfo.Exists; }
        
        public DateTime LastFetched { get; private set; }

        public ManagedFile(string path)
        {
            FileInfo = new FileInfo(path);
            LastFetched = DateTime.Now;
            locker = new object();
        }

        public void Dispose()
        {
            FileInfo = null;
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        public void Delete()
        {
            // Delete the file.
            FileInfo.Delete();
        }

        /// <summary>
        /// Recreates the file.
        /// </summary>
        public void ReCreate()
        {
            if (Exists)
            {
                // Remove the file...
                FileInfo.Delete();
            }

            // ...and recreate it!
            FileInfo.Create();
        }

        /// <summary>
        /// Moves the file to a new location.
        /// </summary>
        /// <param name="destFileName">New location</param>
        public void MoveTo(string destFileName) => MoveTo(destFileName);

        /// <summary>
        /// Refreshes the file.
        /// </summary>
        public void Refresh() => FileInfo.Refresh();

        /// <summary>
        /// Reads the contents of the file and returns them as string.
        /// </summary>
        /// <returns></returns>
        public string ReadStringData() 
        {
            lock (locker)
            {
                string data;

                StreamReader reader = new StreamReader(FullName);
                data = reader.ReadToEnd();
                reader.Close();
                
                LastFetched = DateTime.Now;
                return data;
            }
        }

        /// <summary>
        /// Reads the json data from the file and returns it.
        /// </summary>
        /// <typeparam name="T">The .NET type the json will be converted to.</typeparam>
        /// <returns></returns>
        public object ReadJsonData<T>()
        {
            lock (locker)
            {
                object json;

                using (StreamReader reader = new StreamReader(FullName))
                {
                    try
                    {
                        string content = reader.ReadToEnd();
                        json = JsonConvert.DeserializeObject(content, typeof(T));
                    }
                    catch (Exception)
                    {
                        json = null;
                    }
                }

                LastFetched = DateTime.Now;
                return json;
            }
        }

        /// <summary>
        /// Writes the given string to the file.
        /// NOTE: Use lock if you are using this multithreaded! It can cause data corruption if multiple threads try to access the file at once!
        /// </summary>
        /// <param name="data"></param>
        public void WriteStringData(string data) 
        {
            lock (locker)
            {
                StreamWriter writer = new StreamWriter(FullName);
                writer.Write(data);
            }
        }

        /// <summary>
        /// Writes the json data of the given object to the file.
        /// NOTE: Use lock if you are using this multithreaded! It can cause data corruption if multiple threads try to access the file at once!
        /// </summary>
        /// <param name="data">Object that will be written to the file as json.</param>
        public void WriteJsonData(object data) 
        {
            WriteJsonData(data, Formatting.None);
        }

        /// <summary>
        /// Writes the json data of the given object to the file with custom formatting.
        /// NOTE: Use lock if you are using this multithreaded! It can cause data corruption if multiple threads try to access the file at once!
        /// </summary>
        /// <param name="data">Object that will be written to the file as json.</param>
        public void WriteJsonData(object data, Formatting formatting)
        {
            lock (locker)
            {
                using (StreamWriter writer = new StreamWriter(FullName))
                {
                    writer.WriteLine(JsonConvert.SerializeObject(data, formatting));
                }
            }
        }
    }
}
