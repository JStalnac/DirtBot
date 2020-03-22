using Newtonsoft.Json;

namespace DirtBot.Commands
{
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public struct ModuleData
    {
        [JsonProperty("name", Required = Required.Always)]
        public readonly string Name { get; }
        [JsonProperty("id", Required = Required.Always)]
        public dynamic Value { get; set; }
        [JsonProperty("doc")]
        public readonly string DocString { get; }

        public ModuleData(string name, dynamic value, string docString = null)
        {
            Name = name;
            Value = value;
            DocString = docString;
        }
    }
}
