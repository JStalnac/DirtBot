using System;
using System.Collections.Generic;
using System.IO;

namespace DirtBot.Database.FileManagement
{
    public static class FileManager
    {
        static object locker = new object();
        static Dictionary<string, ManagedDirectory> registeredDirectories = new Dictionary<string, ManagedDirectory>();

        /// <summary>
        /// Loads the files from a directory to a ManagedDirectory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ManagedDirectory LoadDirectory(string path)
        {
            lock (locker)
            {
                if (!Directory.Exists(path))
                {
                    throw new DirectoryNotFoundException($"Unable to load files from '{path}'. That folder does not exist.");
                }

                List<ManagedFile> files = new List<ManagedFile>();
                List<ManagedDirectory> directories = new List<ManagedDirectory>();

                foreach (string filename in Directory.EnumerateFiles(path))
                {
                    files.Add(new ManagedFile(filename));
                }

                foreach (string directoryName in Directory.EnumerateDirectories(path))
                {
                    ManagedDirectory directory = LoadDirectory(directoryName);
                    directories.Add(directory);
                }

                return new ManagedDirectory(path, files.ToArray(), directories.ToArray());
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
            if (name is null)
                throw new ArgumentNullException(nameof(name));
            if (directory is null || directory.Files is null || directory.Directories is null)
                throw new ArgumentNullException(nameof(directory));

            registeredDirectories.Add(name, directory);
        }

        /// <summary>
        /// Gets a directory by name from the registered directories.
        /// </summary>
        /// <param name="name">Name to search for.</param>
        /// <returns></returns>
        public static ManagedDirectory GetRegistedDirectory(string name)
        {
            ManagedDirectory directory = registeredDirectories[name];
            directory.Refresh();
            return directory;
        }
    }
}
