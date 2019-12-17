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
        [JsonProperty(Required = Required.AllowNull)]
        private static string token = "";
        [JsonProperty(Required = Required.AllowNull)]
        private static string ownerId = "";
        [JsonProperty(Required = Required.Always)]
        private static string prefix = "";
        [JsonProperty(Required = Required.Always)]
        private static int cacheUpdateInterval = 20000;
        [JsonProperty(Required = Required.Always)]
        private static string databaseAddress = "localhost";
        [JsonProperty(Required = Required.Always)]
        private static string databaseName = "database1234";
        [JsonProperty(Required = Required.Always)]
        private static string databaseUsername = "myUsername123";
        [JsonProperty(Required = Required.Always)]
        private static string databasePassword = "thisisaverybadpassword123";
#pragma warning restore IDE0044 // Add readonly modifier
        
        public static string Token { get => token; }
        public static string OwnerId { get => ownerId; }
        public static string Prefix { get => prefix; }
        public static int CacheUpdateInterval { get => cacheUpdateInterval; }
        public static string DatabaseAddress { get { return databaseAddress; } }
        public static string DatabaseName { get { return databaseName; } }
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
                StreamWriter writer = new StreamWriter("config.example.json");
                writer.WriteLine(JsonConvert.SerializeObject(new Config(), Formatting.Indented));
                writer.Close();

                Console.WriteLine($"Unable to read config.json! Error: {e.Message}\nPlease go check it from the application root! An example config has been generated!");
                Environment.Exit(1);
            }
        }
    }
}
