using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DirtBot.DataBase
{
    public class GuildLookUp<TKey, TValue> : DataFile, ILookupTable<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _lookup;

        public TValue this[TKey key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string FileName => throw new NotImplementedException();

        public GuildLookUp()
        {
            LoadData();
        }

        /// <summary>
        /// Saves the lookup data to the disk.
        /// </summary>
        protected override void SaveData()
        {
            File.WriteAllText(FullName, SaveToString());
            //Log.WriteLine($"{this.GetType().Name} was saved!");
        }
        /// <summary>
        /// Loads the lookup data from the disk.
        /// </summary>
        protected override void LoadData()
        {
            // Check if the file exists
            if (!File.Exists(FullName))
            {
                EnsureStorageFile();
                //if (RequiresTemplateFile)
                //{
                //    // If the file requires a template, load the template
                //    string templatePath = TEMPLATE_IDENTIFIER + FullName;
                //    if (File.Exists(templatePath))
                //    {
                //        string filename = templatePath.Substring(TEMPLATE_IDENTIFIER.Length);
                //        File.Copy(templatePath, FullName);
                //    }
                //    else
                //    {
                //        throw new FileNotFoundException($"Template file for { GetType().Name } was requested, but not found.");
                //    }
                //}
                //else
                //{
                //    // If not, ensure an empty storage file
                //    EnsureStorageFile();
                //}
            }

            string json = File.ReadAllText(FullName);
            LoadFromString(json);
        }
        /// <summary>
        /// Ensures that a lookup file exist at the filepath.
        /// </summary>
        protected override void EnsureStorageFile()
        {
            if (!File.Exists(FullName))
                File.WriteAllText(FullName, "{}");
        }

        /// <summary>
        /// Adds an element to the table.
        /// </summary>
        public virtual void Add(TKey key, TValue value)
        {
            _lookup.Add(key, value);
            SaveData();
        }
        /// <summary>
        /// Retrieves an element from the table by its key.
        /// </summary>
        public virtual TValue Get(TKey key)
        {
            return _lookup[key];
        }
        /// <summary>
        /// Retrieves an element from the table by its key, or returns null if the element does not exist.
        /// </summary>
        public virtual TValue GetOrDefault(TKey key)
        {
            if (Has(key)) return Get(key);
            else return default;
        }
        /// <summary>
        /// Sets (updates) an element in the table.
        /// </summary>
        public virtual void Set(TKey key, TValue value)
        {
            _lookup[key] = value;
            SaveData();
        }
        /// <summary>
        /// Checks if the table contains the specified key.
        /// </summary>
        public virtual bool Has(TKey key)
        {
            return _lookup.ContainsKey(key);
        }
        /// <summary>
        /// Removes the element with the specified key from the table.
        /// </summary>
        public virtual bool Remove(TKey key)
        {
            bool exists = _lookup.Remove(key);
            SaveData();

            return exists;
        }

        /// <summary>
        /// Clears all elements from the table.
        /// </summary>
        public virtual void Clear()
        {
            _lookup.Clear();
            SaveData();
        }

        protected override string SaveToString() => JsonConvert.SerializeObject(_lookup, Formatting.Indented);
        protected override void LoadFromString(string value) => _lookup = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(value);
    }
}
