using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DirtBot
{
    public struct Config
    {
        [JsonProperty(Required = Required.AllowNull)]
        public static string Token { get; private set; } = "";
        [JsonProperty(Required = Required.AllowNull)]
        public static string OwnerId { get; private set; } = "";
        [JsonProperty(Required = Required.Always)]
        public static string Prefix { get; private set; } = "";
        [JsonProperty(Required = Required.Always)]
        public static int CacheUpdateInterval { get; private set; } = 20000;
        [JsonProperty(Required = Required.Always)]
        public static bool LogTraces { get; private set; } = true;

        [JsonProperty]
        public static List<Emoji> Emotes { get; private set; } = new List<Emoji>() { /* Defaults can be listed here */ };

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
