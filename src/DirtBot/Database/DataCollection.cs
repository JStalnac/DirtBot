using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DirtBot.Database
{
    [Serializable]
    public class DataCollection<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEquatable<DataCollection<TKey, TValue>>
    {
        protected IDictionary<TKey, TValue> data;

        public IEnumerable<TKey> Keys { get => data.Keys; }
        public IEnumerable<TValue> Values { get => data.Values; }

        internal DataCollection(IDictionary<TKey, TValue> data)
        {
            this.data = data;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Checks if the other data collection has all the same keys.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals([AllowNull] DataCollection<TKey, TValue> other)
        {
            if (other is null) return false;
            foreach (var key in Keys)
            {
                if (!other.data.ContainsKey(key))
                {
                    return false;
                }
            }
            foreach (var key in other.Keys)
            {
                if (!data.ContainsKey(key))
                {
                    return false;
                }
            }
            return true;
        }

        public TValue this[TKey key]
        {
            get => data[key];
            set => data[key] = value;
        }

        public bool ContainsKey(TKey key) => data.ContainsKey(key);

        public void Add(TKey key, TValue value) => data.Add(key, value);

        public void Remove(TKey key) => data.Remove(key);

        public void Clear() => data.Clear();
    }
}
