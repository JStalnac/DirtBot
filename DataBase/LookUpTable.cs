﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

/*
 * From BrackeysBot - https://github.com/YilianSource/brackeys-bot/blob/master/Data/LookupTable.cs
 */

namespace DirtBot.DataBase
{
    /// <summary>
    /// Provides a lookup table that will be serialized into a JSON file.
    /// </summary>
    public abstract class LookupTable<TKey, TValue> : DataFile, ILookupTable<TKey, TValue>, ILookup
    {
        protected const string FileType = "json";
        protected const string TEMPLATE_IDENTIFIER = "template-";

        protected Dictionary<TKey, TValue> Table => _lookup;

        string ILookup.FullName { get => FileName; set { } }
        string ILookup.FileType => FileType;

        private Dictionary<TKey, TValue> _lookup;

        public LookupTable()
        {
            LoadData();
        }

        /// <summary>
        /// Serializes the contents of the lookup table to a string.
        /// </summary>
        protected override string SaveToString()
            => JsonConvert.SerializeObject(_lookup, Formatting.Indented);
        /// <summary>
        /// Deserializes the contents of the lookup table from a string.
        /// </summary>
        protected override void LoadFromString(string value)
            => _lookup = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(value);

        public virtual TValue this[TKey index]
        {
            get => _lookup[index];
            set => _lookup[index] = value;
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

        void ILookup.LoadData() => LoadData();
        void ILookup.EnsureStorageFile() => EnsureStorageFile();
    }
}
