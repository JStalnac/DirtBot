using System;
using System.Collections.Generic;

namespace DirtBot.Database
{
    [Serializable]
    public class ReadOnlyDataCollection<TKey, TValue> : DataCollection<TKey, TValue>
    {
        public ReadOnlyDataCollection(IDictionary<TKey, TValue> data) : base(data) { }

        public new TValue this[TKey key]
        {
            get => data[key];
        }

        public DataCollection<TKey, TValue> ToWritable() => this as DataCollection<TKey, TValue>;
    }
}
