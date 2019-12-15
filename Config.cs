using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace DirtBot
{
    /// <summary>
    /// Bot configuration
    /// </summary>
    public class Config
    {
        [JsonProperty]
        public string token;
        [JsonProperty]
        public string ownerId;
        [JsonProperty]
        public string prefix;
        [JsonProperty]
        public int cacheUpdateInterval = 20000;
        [JsonProperty]
        public string databaseUsername;
        [JsonProperty]
        public string databasePassword;

        public static Config LoadConfig()
        {
            try
            {
                if (!File.Exists("config.json"))
                {
                    Console.WriteLine("config.json not found! Restoring default config!");
                    File.WriteAllText("config.json", JsonConvert.SerializeObject(new Config(), Formatting.Indented));
                    Environment.Exit(1);
                    return new Config();
                }

                string json = File.ReadAllText("config.json");
                Config config = JsonConvert.DeserializeObject<Config>(json);

                if (config is null) 
                {
                    Console.WriteLine("Failed to read config.json! Restoring deafult config!");
                    File.WriteAllText("config.json", JsonConvert.SerializeObject(new Config(), Formatting.Indented));
                    Environment.Exit(1);
                    return new Config();
                }

                return config;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to read config.json! Error: {e.Message}");
                Environment.Exit(1);
                return new Config();
            }
        }
    }
}
