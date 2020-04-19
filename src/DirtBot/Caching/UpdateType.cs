namespace DirtBot.Caching
{
    public enum UpdateType
    {
        /// <summary>
        /// Never removes the caches.
        /// </summary>
        Never,
        /// <summary>
        /// Removes the caches based on last access time.
        /// </summary>
        LastAccess,
        /// <summary>
        /// Removes the caches based on their remove time.
        /// Field removeAfterSeconds is not used.
        /// </summary>
        RemoveTime,
        /// <summary>
        /// Removes the caches based on the time they were created.
        /// </summary>
        CreationTime,
    }

}
