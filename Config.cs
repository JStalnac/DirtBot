using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace DirtBot
{
    /// <summary>
    /// Bot configuration
    /// </summary>
    public struct Config
    {
#pragma warning disable IDE0044 // Add readonly modifier
        [JsonProperty]
        private static string token;
        [JsonProperty]
        private static string ownerId;
        [JsonProperty]
        private static string prefix;
        [JsonProperty]
        private static int cacheUpdateInterval = 20000;
        [JsonProperty]
        private static string databaseUsername;
        [JsonProperty]
        private static string databasePassword;
#pragma warning restore IDE0044 // Add readonly modifier
        public static string Token { get => token; }
        public static string OwnerId { get => ownerId; }
        public static string Prefix { get => prefix; }
        public static int CacheUpdateInterval { get => cacheUpdateInterval; }
        public static string DatabaseUsername { get => databaseUsername; }
        public static string DatabasePassword { get => databasePassword; }

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
                Console.WriteLine($"Unable to read config.json! Error: {e.Message}");
                Environment.Exit(1);
            }
        }
    }
}
