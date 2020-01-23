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
            return Files.GetEnumerator();
        }
        
        public ManagedFile this[int index] 
        {
            get => Files[index];
            set => Files[index] = value;
        }

        public ManagedFile this[string filename] 
        {
            get => GetFile(filename);
            set => Files[IndexOf(filename)] = value;
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
        /// Gets the index of a file in Files.
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

            throw new FileNotFoundException($"No file named '{filename}'");
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
            File.Create(DirectoryInfo.FullName + filename);
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
