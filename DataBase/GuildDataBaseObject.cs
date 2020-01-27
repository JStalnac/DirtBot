using Newtonsoft.Json;

namespace DirtBot.DataBase
{
    public class GuildDataBaseObject
    {
        [JsonProperty]
        public ulong Id { get; private set; }
        [JsonProperty]
        public string Prefix { get; set; }

        public GuildDataBaseObject(ulong id, string prefix)
        {
            Id = id;
            Prefix = prefix;
        }
    }
}
