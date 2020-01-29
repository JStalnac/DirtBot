using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DirtBot.DataBase
{
    public abstract class DirectoryLookUp<TKey, TValue> : LookupTable<TKey, TValue>
    {
        public override string FileName => null;
        public virtual string DirectoryName { get; }

        /// <summary>
        /// Loads the lookup data from the disk.
        /// </summary>
        protected override void LoadData()
        {
            if (!Directory.Exists(DirectoryName)) 
            {
                Directory.CreateDirectory(DirectoryName);
            }
            else
            {
                foreach (string item in Directory.EnumerateDirectories(DirectoryName))
                {

                }
            }

            //string json = File.ReadAllText(FullName);
            //LoadFromString(json);
        }

        protected override string SaveToString() => throw new NotImplementedException("Can not save directory to string.");

        protected override void LoadFromString(string value) => throw new NotImplementedException("Can not load directory from string.");
    }
}
