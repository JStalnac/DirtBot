namespace DirtBot.Caching
{
    /// <summary>
    /// Interface for saving service information to CacheSave objects.
    /// </summary>
    public interface ICacheDataObject
    {
        /// <summary>
        /// Default value for this cache object.
        /// </summary>
        object DeafaultValue { get; }
        /// <summary>
        /// Names make it easier to find objects.
        /// </summary>
        string Name { get; set; }
    }

    public class CacheDataObject : ICacheDataObject
    {
        public int defaultValue = 0;
        private int value;
        private string name;

        public CacheDataObject(string name)
        {
            this.name = name;
            value = defaultValue;
        }

        public object DeafaultValue 
        {
            get { return defaultValue; }
        }

        public int Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public string Name 
        {
            get { return name; }
            set { name = value; }
        }
    }
}
