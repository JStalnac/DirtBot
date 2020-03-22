using System.Collections.Generic;

namespace DirtBot.Helpers
{
    public static class DictionaryHelper
    {
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultVal = default)
        {
            if (dictionary.TryGetValue(key, out TValue value))
            {
                return value;
            }
            return defaultVal;
        }
    }
}
