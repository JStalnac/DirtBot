using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DirtBot.DataBase
{
    internal class GuildDataBaseObject
    {
        [JsonProperty]
        public ulong Id { get; private set; }
        [JsonProperty]
        public string DisplayName { get; private set; }
        [JsonProperty]
        public string Prefix { get; private set; }

        public GuildDataBaseObject(ulong id, string displayName, string prefix)
        {
            Id = id;
            DisplayName = displayName;
            Prefix = prefix;
        }
    }
}
