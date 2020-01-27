using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DirtBot.Caching
{
    /// <summary>
    /// Represents a cached guild.
    /// </summary>
    public class CacheSave
    {
        private Dictionary<string, List<ICacheDataObject>> serviceData = new Dictionary<string, List<ICacheDataObject>>();

        public CacheSave(ulong id, string name = "An object", bool remove = true, uint removeAfter = 300)
        {
            Id = id.ToString();
            Name = name;
            CreationTime = DateTime.Now;
            Remove = remove;
            RemoveAfter = removeAfter;
        }

        #region Properties

        #region Id
        /// <summary>
        /// The guild's id
        /// </summary>
        public string Id { get; set; }
        #endregion

        #region Name
        public string Name { get; set; }
        #endregion

        #region Remove
        public bool Remove { get; set; }
        #endregion

        #region CreationTime
        /// <summary>
        /// Time that the cache was made. Objects will be removed from cache when unused.
        /// </summary>
        public DateTime CreationTime { get; set; }
        #endregion

        #region RemoveAfter
        /// <summary>
        /// Time in seconds when this CacheSave will be removed after adding it.
        /// </summary>
        public uint RemoveAfter { get; set; }
        #endregion

        #endregion

        /// <summary>
        /// Returns the object under the key from data with the given name. Returns fallback if there is no key for that.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        public async Task<ICacheDataObject> GetFromDataUnderKeyAsync(string key, string name, ICacheDataObject fallback)
        {
            List<ICacheDataObject> dataUnderKey;

            try
            {
                dataUnderKey = serviceData[key];
            }
            catch (KeyNotFoundException)
            {
                await AddToDataAsync(key, fallback);
                return fallback;
            }
            
            foreach (ICacheDataObject obj in dataUnderKey)
            {
                if (obj.Name == name) 
                {
                    return obj;
                }
            }
            dataUnderKey.Add(fallback);
            return fallback;
        }

        /// <summary>
        /// Gets the object from the cache by a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<ICacheDataObject>> GetFromDataAsync(string key)
        {
            try
            {
                return serviceData[key];
            }
            catch (NullReferenceException)
            {
                // There is no cached data for that key...
                // Make it and leave.
                serviceData.Add(key, new List<ICacheDataObject>());
                return null;
            }
        }

        /// <summary>
        /// Adds an object to the data
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cacheObject"></param>
        /// <returns></returns>
        public async Task AddToDataAsync(string key, ICacheDataObject cacheObject)
        {
            serviceData.Add(key, new List<ICacheDataObject>());
            serviceData[key].Add(cacheObject);
        }

        /// <summary>
        /// Changes the disposing time to the current time to keep the object in cache for longer.
        /// </summary>
        /// <returns></returns>
        public async Task SetRemoveTime()
        {
            CreationTime = DateTime.Now;
        }
    }
}
