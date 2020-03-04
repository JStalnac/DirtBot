using Newtonsoft.Json;

namespace DirtBot.Commands
{
	[JsonObject(MemberSerialization = MemberSerialization.OptOut)]
	public struct CommandData
	{
		[JsonProperty(Required = Required.Always)]
		public readonly string Name { get; }
		[JsonProperty(Required = Required.Always)]
		public dynamic Value { get; set; }
		public readonly string DocString { get; }

		public CommandData(string name, dynamic value, string docString = "")
		{
			Name = name;
			Value = value;
			DocString = docString;
		}
	}
}
