using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace DirtBot
{
    /// <summary>
    /// Bot config
    /// </summary>
    public class Config
    {
        string token;
        string ownerId;
        string prefix;
        int cacheUpdateInterval;

        // Just hardcode the default config.
        private string defaultConfig = "{\n\t\"token\": \"\",\n\t\"ownerId\": \"\",\n\t\"prefix\": \"\",\n\t\"cacheUpdateInterval\": 20000\n}";

        public Config()
        {
            try
            {
                StreamReader reader = new StreamReader("config.json");
                string json = reader.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                // Bot token
                token = data["token"].ToString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    Console.WriteLine("Inproper token was supplied. Please go fill it in config.json. (application root)");
                    Environment.Exit(-1);
                }

                // Owner id if we will need it, which we will.
                ownerId = data["ownerId"].ToString();
                // Prefix for convenience
                prefix = data["prefix"].ToString();
                // The interval that the cache will be updated in milliseconds
                cacheUpdateInterval = int.Parse(data["cacheUpdateInterval"].ToString());
            }
            catch (FileNotFoundException e)
            {
                // The message has the error message ready for us so we won't have to type it lol
                Console.WriteLine($"{e.Message}\nCreating it...\n");

                // Restore config.
                RestoreDefaultConfig();
                // Exit
                Environment.Exit(1);
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Unable to read config.json! Error: {e.Message}");
                Environment.Exit(1);
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine($"Unable to read config.json! Error: {e.Message}\nRestoring it to default!\n");
                RestoreDefaultConfig();
                Environment.Exit(1);
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine($"Unable to read config.json! Error: {e.Message}\nRestoring it to default!\n");
                RestoreDefaultConfig();
                Environment.Exit(1);
            }
        }

        private void RestoreDefaultConfig()
        {
            StreamWriter writer = new StreamWriter("config.json");

            // Try to write
            try { writer.Write(defaultConfig); }
            // Error...
            catch (Exception e)
            {
                Console.WriteLine($"Unable to restore config. Error: {e.Message}");
                Environment.Exit(1);  
            }
            // Finally close the file. Oh wait we don't even have because the environment will exit...
            finally { writer.Close(); }
        }

        #region Token
        /// <summary>
        /// The bots token
        /// </summary>
        public string Token { get { return token; } }
        #endregion

        #region Prefix
        /// <summary>
        /// The command prefix for the bot
        /// </summary>
        public string Prefix
        {
            get { return prefix; }
            set { prefix = value; }
        }
        #endregion

        #region OwnerId
        /// <summary>
        /// The bots owners id
        /// </summary>
        public string OwnerId { get { return ownerId; } }
        #endregion

        #region CacheUpdateInterval
        /// <summary>
        /// The interval that the cache will be updated at.
        /// </summary>
        public int CacheUpdateInterval 
        {
            get { return cacheUpdateInterval; }
            set { cacheUpdateInterval = value; }
        }
        #endregion
    }
}
