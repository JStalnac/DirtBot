using System.Collections.Generic;
using Discord;
using Discord.WebSocket;

namespace DirtBot.Caching
{
    public class Cache
    {
        // List of cached objects by name which have varriables and stuff of dynamic type.
        public static Dictionary<string, Dictionary<string, dynamic>> Caches { get; } = new Dictionary<string, Dictionary<string, dynamic>>();
        
        /// <summary>
        /// Gets the cached guild by id from message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Dictionary<string, dynamic> this[IMessage message] 
        {
            get 
            {
                try
                {
                    return Caches[(message.Channel as SocketGuildChannel).Guild.Id.ToString()];
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Gets the cached guild by guild id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Dictionary<string, dynamic> this[ulong id] 
        {
            get => this[id.ToString()];
        }
        /// <summary>
        /// Gets the cached guild by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Dictionary<string, dynamic> this[string name] 
        {
            get 
            {
                try
                {
                    return Caches[name];
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
            }
        }
    }
}
