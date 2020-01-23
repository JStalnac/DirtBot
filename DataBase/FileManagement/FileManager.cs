using System.IO;
using System.Collections.Generic;

namespace DirtBot.DataBase.FileManagement
{
    public static class FileManager
    {
        static Dictionary<string, ManagedDirectory> RegisteredDirectories = new Dictionary<string, ManagedDirectory>();

        /// <summary>
        /// Loads the files from a directory to a ManagedDirectory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ManagedDirectory LoadDirectory(string path) 
        {
            if (!Directory.Exists(path)) 
            {
                throw new DirectoryNotFoundException($"Unable to load files from '{path}'. That folder does not exist.");
            }

            List<ManagedFile> files = new List<ManagedFile>();

            foreach (string filename in Directory.EnumerateFiles(path))
            {
                ManagedFile managedFile = new ManagedFile(filename);
                files.Add(managedFile);
            }

            return new ManagedDirectory(path, files.ToArray());
        }

        /// <summary>
        /// Loads the file from the given location.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static ManagedFile LoadFile(string filepath) 
        {
            if (!File.Exists(filepath)) 
            {
                throw new FileNotFoundException($"File '{filepath}' not found!");
            }
            else
            {
                return new ManagedFile(filepath);
            }
        }

        /// <summary>
        /// Adds the directory to a list of registered directories.
        /// </summary>
        /// <param name="directory">The directory that will be registered</param>
        /// <returns></returns>
        public static void RegisterDirectory(ManagedDirectory directory)
        {
            RegisterDirectory(directory.DirectoryInfo.Name, directory);
        }
        /// <summary>
        /// Adds the directory to a list of registered directories.
        /// </summary>
        /// <param name="name">The name that the directory will be accessed with.</param>
        /// <param name="directory">The directory that will be registered</param>
        /// <returns></returns>
        public static void RegisterDirectory(string name, ManagedDirectory directory)
        {
            RegisteredDirectories.Add(name, directory);
        }

        /// <summary>
        /// Gets a directory by name from the registered directories.
        /// </summary>
        /// <param name="name">Name to search for.</param>
        /// <returns></returns>
        public static ManagedDirectory GetRegistedDirectory(string name)
        {
            ManagedDirectory directory = RegisteredDirectories[name];
            directory.Refresh();
            return directory;
        }
    }
}
