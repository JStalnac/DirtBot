using DirtBot.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DirtBot.Caching
{
    public class InstanceCache<TKey, TValue>
    {
        public event CacheRemoveEventHandler OnCacheRemove;

        Dictionary<TKey, DataPair> dataPairs = new Dictionary<TKey, DataPair>();

        /// <summary>
        /// The time in seconds that caches will be removed after a condition is met.
        /// NOTE: Only used with the built-in cache updaters.
        /// </summary>
        public double removeAfterSeconds = 180d;

        public delegate UpdateResult Update(DataPair pair, DateTime now);
        public delegate void CacheRemoveEventHandler(TKey key, bool failed, Exception exception = null);
        Update update;
        string name;

        public InstanceCache(Update update, string name = "Cache")
        {
            this.update = update;
            this.name = name;

            OnCacheRemove += InstanceCache_OnCacheRemove;
            Thread thread = new Thread(CacheUpdate);
            thread.Name = $"InstanceCache<{typeof(TKey)}, {typeof(TValue)}> '{name}'";
            thread.Start();
        }

        public InstanceCache(UpdateType updateType, string name = "Cacher")
        {
            switch (updateType)
            {
                case UpdateType.Never:
                    update = new Update((pair, now) =>
                    {
                        return UpdateResult.Keep;
                    });
                    break;
                case UpdateType.LastAccess:
                    update = new Update((pair, now) =>
                    {
                        if (pair.TimeSinceLastAccess(now) > removeAfterSeconds)
                        {
                            return UpdateResult.Remove;
                        }
                        return UpdateResult.Keep;
                    });
                    break;
                case UpdateType.RemoveTime:
                    update = new Update((pair, now) =>
                    {
                        if (pair.GetTimeToRemove(now) < 0d)
                        {
                            return UpdateResult.Remove;
                        }
                        return UpdateResult.Keep;
                    });
                    break;
                case UpdateType.CreationTime:
                    update = new Update((pair, now) =>
                    {
                        if (pair.GetAge(now) > removeAfterSeconds)
                        {
                            return UpdateResult.Remove;
                        }
                        return UpdateResult.Keep;
                    });
                    break;
                default:
                    break;
            }

            OnCacheRemove += InstanceCache_OnCacheRemove;

            this.name = name;
            Thread thread = new Thread(CacheUpdate);
            thread.Name = $"InstanceCache<{typeof(TKey)}, {typeof(TValue)}> '{name}'";
            thread.Start();
        }

        private void InstanceCache_OnCacheRemove(TKey key, bool failed, Exception exception = null)
        {
            if (failed)
            {
                Logger.Log($"Cache '{name}' failed to update the key '{key}'. See log for details", fore: ConsoleColor.Red);
                Logger.WriteLogFile($"Cache '{name}' failed to update the key {key}. Exception:\n{exception}");
            }
        }

        public class DataPair
        {
            public KeyValuePair<TKey, TValue> Pair;
            public DateTime removeTime;
            public readonly DateTime creationTime;
            public DateTime lastAccess;

            public DataPair(double removeAfterSeconds, TKey key, TValue value)
            {
                Pair = new KeyValuePair<TKey, TValue>(key, value);
                removeTime = DateTime.Now.AddSeconds(removeAfterSeconds);
                lastAccess = DateTime.Now;
            }

            /// <summary>
            /// Sets the last access time to the current time and returns the value.
            /// </summary>
            /// <returns></returns>
            public TValue GetValue()
            {
                lastAccess = DateTime.Now;
                return Pair.Value;
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
            /// <summary>
            /// Gets time time in seconds since the last time the pair was accessed.
            /// </summary>
            /// <param name="now"></param>
            /// <returns></returns>
            public double TimeSinceLastAccess(DateTime now) => (now - lastAccess).TotalSeconds;
        }

        public void Add(TKey key, TValue value, double removeAfterSeconds = 300) => dataPairs.Add(key, new DataPair(removeAfterSeconds, key, value));

        public void Remove(TKey key) => dataPairs.Remove(key);

        public TValue Get(TKey key) => dataPairs[key].GetValue();

        /// <summary>
        /// Sets the value for the key. Creates a new key if the specified key doesn't exist.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(TKey key, TValue value, double removeAfterSeconds = 300)
        {
            try
            {
                // Really painful...
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
                    UpdateResult result = update.Invoke(pair, now);
                    switch (result)
                    {
                        case UpdateResult.Keep:
                            continue;
                        case UpdateResult.Remove:
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
                                OnCacheRemove.Invoke(pair.Pair.Key, failed, exception);
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
