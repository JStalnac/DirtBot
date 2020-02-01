﻿/*
 * From BrackeysBot - https://github.com/YilianSource/brackeys-bot/blob/master/Data/ILookupTable.cs
 */

using System;

namespace DirtBot.DataBase
{
    /// <summary>
    /// Indicates a lookup table.
    /// </summary>
    public interface ILookupTable<TKey, TValue>
    {
        /// <summary>
        /// Gets or sets a value in the lookup table.
        /// </summary>
        TValue this[TKey key] { get; set; }

        /// <summary>
        /// Adds a value to the lookup table.
        /// </summary>
        void Add(TKey key, TValue value);

        /// <summary>
        /// Gets a value from the lookup table.
        /// </summary>
        TValue Get(TKey key);

        /// <summary>
        /// Gets a value from the lookup table. Returns the default value for <see cref="TValue"/> if the key doesn't exist.
        /// </summary>
        TValue GetOrDefault(TKey key);

        /// <summary>
        /// Sets a value in the lookup table.
        /// </summary>
        void Set(TKey key, TValue value);

        /// <summary>
        /// Checks if the table contains the specified key.
        /// </summary>
        bool Has(TKey key);

        /// <summary>
        /// Removes the specified key from the lookup. Returns false if the key doesn't exist.
        /// </summary>
        bool Remove(TKey key);
    }
}
