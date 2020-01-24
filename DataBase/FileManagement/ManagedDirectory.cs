using System;
using System.IO;
using System.Collections;

namespace DirtBot.DataBase.FileManagement
{
    public struct ManagedDirectory : IDisposable, IEnumerable
    {
        public DirectoryInfo DirectoryInfo { get; private set; }
        public ManagedFile[] Files { get; private set; }

        public ManagedDirectory(string path, ManagedFile[] files)
        {
            DirectoryInfo = new DirectoryInfo(path);
            Files = files;
        }

        public void Dispose()
        {
            Files = null;
            DirectoryInfo = null;
        }

        public IEnumerator GetEnumerator()
        {
            Refresh();
            return Files.GetEnumerator();
        }
        
        public ManagedFile this[int index] 
        {
            get => Files[index];
            set 
            {
                Files[index] = value;
                Refresh();
            }
        }

        public ManagedFile this[string filename] 
        {
            get => GetFile(filename);
            set 
            {
                Files[IndexOf(filename)] = value;
                Refresh();
            }
        }

        /// <summary>
        /// Gets the file by name.
        /// </summary>
        /// <param name="filename">Filename to search for.</param>
        /// <returns></returns>
        public ManagedFile GetFile(string filename) 
        {
            // Loop through the files in this ManagedDirectory.
            foreach (ManagedFile file in Files)
            {
                // Check if the filename is correct
                if (file.FullName == filename || file.Name == filename)
                {
                    // Return the file
                    return file;
                }
            }

            // There is no file...
            throw new FileNotFoundException($"No file named '{filename}'");
        }

        /// <summary>
        /// Gets the index of a file in Files. Returns -1 if no file is found
        /// </summary>
        /// <param name="filename">Filename to search for.</param>
        /// <returns></returns>
        public int IndexOf(string filename) 
        {
            for (int i = 0; i < Files.Length; i++)
            {
                if (Files[i].Name == filename || Files[i].FullName == filename) 
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Reloads this directory.
        /// </summary>
        public void Refresh() 
        {
            this = FileManager.LoadDirectory(DirectoryInfo.FullName);
        }

        /// <summary>
        /// Deletes the file with the name.
        /// </summary>
        /// <param name="filename">File that will be removed.</param>
        public void DeleteFile(string filename) 
        {
            this[filename].Delete();
            this[filename].Dispose();
            Refresh();
        }

        /// <summary>
        /// Deletes this directory and all the contents of it.
        /// </summary>
        public void DeleteDirectory() 
        {
            foreach (ManagedFile file in Files)
            {
                DeleteFile(file.FullName);
            }

            DirectoryInfo.Delete();
        }

        /// <summary>
        /// Creates a file to this directory
        /// </summary>
        /// <param name="filename"></param>
        public void AddFile(string filename) 
        {
            StreamWriter writer = new StreamWriter(DirectoryInfo.FullName + filename);
            writer.Close();
            // Refreshing is important! Without it the new file won't be found!
            Refresh();
        }

        /// <summary>
        /// Creates a subdirectory to this directory
        /// </summary>
        /// <param name="name"></param>
        public void CreateSubdirectory(string name) 
        {
            DirectoryInfo.CreateSubdirectory(name);
        }
    }
}
