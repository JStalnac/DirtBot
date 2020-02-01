using Newtonsoft.Json;

namespace DirtBot.DataBase.DataBaseObjects
{
    public class GuildDataObject : IGuildDataObject
    {
        [JsonProperty]
        public ulong Id { get; private set; }
        [JsonProperty]
        public string Prefix { get; set; }

        public GuildDataObject(ulong id, string prefix)
        {
            Id = id;
            Prefix = prefix;
        }
    }
}
