using System;
using System.Collections.Generic;
using System.Threading;

namespace DirtBot.Caching
{
    public class InstanceCache
    {
        public enum UpdateResult
        {
            Keep,
            Remove
        }
    }

    public class InstanceCache<TKey, TValue>
    {
        public delegate InstanceCache.UpdateResult Update(DataPair pair, DateTime now);
        public delegate void CacheRemoveEventHandler(TKey key, bool failed, Exception exception = null);
        Update update;

        public event CacheRemoveEventHandler OnCacheRemove;

        Dictionary<TKey, DataPair> dataPairs = new Dictionary<TKey, DataPair>();

        public InstanceCache(Update update)
        {
            this.update = update;
            
            Thread thread = new Thread(CacheUpdate);
            thread.Start();
        }

        public class DataPair
        {
            public KeyValuePair<TKey, TValue> Pair;
            public DateTime removeTime;
            public readonly DateTime creationTime;

            public DataPair(double removeAfterSeconds, TKey key, TValue value)
            {
                Pair = new KeyValuePair<TKey, TValue>(key, value);
                removeTime = DateTime.Now.AddSeconds(removeAfterSeconds);
            }

            /// <summary>
            /// Gets the age of the pair in seconds.
            /// </summary>
            /// <param name="now"></param>
            /// <returns></returns>
            public double GetAge(DateTime now) => (now - creationTime).TotalSeconds;
            /// <summary>
            /// Gets the time in seconds to remove the pair. Positive if it is in the future.
            /// </summary>
            /// <param name="now"></param>
            /// <returns></returns>
            public double GetTimeToRemove(DateTime now) => (removeTime - now).TotalSeconds;
        }

        public void Add(TKey key, TValue value, double removeAfterSeconds = 300) => dataPairs.Add(key, new DataPair(removeAfterSeconds, key, value));
        
        public void Remove(TKey key) => dataPairs.Remove(key);
        
        public TValue Get(TKey key) => dataPairs[key].Pair.Value;
        
        /// <summary>
        /// Sets the value for the key. Creates a new key if the specified key doesn't exist.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(TKey key, TValue value, double removeAfterSeconds = 300)
        {
            try
            {
                DataPair oldPair = dataPairs[key];
                double currentRemoveTime = (oldPair.removeTime - oldPair.creationTime).TotalSeconds;
                DataPair pair = new DataPair(currentRemoveTime, key, value);
                dataPairs[key] = pair;
            }
            catch (KeyNotFoundException)
            {
                dataPairs.Add(key, new DataPair(removeAfterSeconds, key, value));
            }
        }

        public TValue this[TKey key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        private void CacheUpdate()
        {
            while (true)
            {
                DateTime now = DateTime.Now;

                foreach (var pair in dataPairs.Values)
                {
                    InstanceCache.UpdateResult result = update.Invoke(pair, now);
                    switch (result)
                    {
                        case InstanceCache.UpdateResult.Keep:
                            continue;
                        case InstanceCache.UpdateResult.Remove:
                            bool failed = false;
                            Exception exception = null;
                            try
                            {
                                Remove(pair.Pair.Key);
                            }
                            catch (Exception e)
                            {
                                failed = true;
                                exception = e;
                            }
                            finally
                            {
                                OnCacheRemove.Invoke(pair.Pair.Key,
                                    failed,
                                    exception);
                            }
                            continue;
                        default:
                            continue;
                    }
                }

                Thread.Sleep(Config.CacheUpdateInterval);
            }
        }
    }
}
