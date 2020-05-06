using Newtonsoft.Json;
using System;
using System.IO;

namespace DirtBot.Core
{
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public struct Config
    {
        [JsonProperty(Required = Required.AllowNull)]
        public static string Token { get; private set; } = "";
        [JsonProperty(Required = Required.Always)]
        public static string Prefix { get; private set; } = "";
        [JsonProperty(Required = Required.Always)]
        public static string RedisUrl { get; set; } = "localhost";

        static Config()
        {
            new Config().Load();
        }

        void Load()
        {
            try
            {
                if (!File.Exists("config.json"))
                {
                    Console.WriteLine("config.json not found! Restoring default config!");
                    File.WriteAllText("config.json", JsonConvert.SerializeObject(new Config(), Formatting.Indented));
                    Environment.Exit(1);
                }

                string json = File.ReadAllText("config.json");
                this = JsonConvert.DeserializeObject<Config>(json);
            }
            catch (Exception e)
            {
                StreamWriter writer = new StreamWriter("config.example.json");
                writer.WriteLine(JsonConvert.SerializeObject(new Config(), Formatting.Indented));
                writer.Close();

                Console.WriteLine($"Unable to read config.json! Error: {e.Message}\nPlease go check it from the application root! An example config has been generated!");
                Environment.Exit(1);
            }
        }
    }
}
