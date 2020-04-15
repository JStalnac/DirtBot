using System.Collections.Generic;

namespace DirtBot.Database
{
    public class DataCollectionBuilder<TKey, TValue>
    {
        Dictionary<TKey, TValue> data = new Dictionary<TKey, TValue>();

        public DataCollectionBuilder<TKey, TValue> Add(TKey key, TValue value)
        {
            data.Add(key, value);
            return this;
        }

        public DataCollectionBuilder<TKey, TValue> Remove(TKey key)
        {
            data.Remove(key);
            return this;
        }

        public DataCollectionBuilder<TKey, TValue> Clear()
        {
            data.Clear();
            return this;
        }

        public ReadOnlyDataCollection<TKey, TValue> BuildReadOnlyDataCollection() => new ReadOnlyDataCollection<TKey, TValue>(data);
        public DataCollection<TKey, TValue> BuildDataCollection() => new DataCollection<TKey, TValue>(data);
    }
}
