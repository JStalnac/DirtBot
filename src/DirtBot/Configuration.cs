using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace DirtBot
{
    public class Configuration
    {
        string filename;
        IDictionary<string, object> data;

        public Configuration(string filename)
        {
            this.filename = filename;
            data = new Dictionary<string, object>();
        }

        public static Configuration LoadConfiguration(string filename)
        {
            var config = new Configuration(filename);

            try
            {
                config.Load();
            }
            catch (FileNotFoundException) { }
            catch (IOException ex)
            {
                var log = new Logger("Configuration Manager");
                log.Error($"Failed to load configuration file {filename}", ex);
            }
            catch (ConfigurationException ex)
            {
                var log = new Logger("Configuration Manager");
                log.Warning($"Failed to load configuration file {filename}", ex);
            }
            return config;
        }

        public void Load()
        {
            // There is no configuration file to load stuff from.
            if (!File.Exists(filename))
                return;

            using (var reader = new StreamReader(filename))
            {
                var d = new Deserializer();
                var newConfig = d.Deserialize<IDictionary<string, object>>(reader);
                data = newConfig ?? throw new ConfigurationException("Unable to parse configuration.");
            }
        }

        public void Save()
        {
            if (!File.Exists(filename))
            {
                string dir = Path.GetDirectoryName(Path.GetFullPath(filename));
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.Create(filename).Close();
            }

            using (var writer = new StreamWriter(filename))
            {
                var s = new Serializer();
                s.Serialize(writer, data);
            }
        }

        public object GetValue(string name)
        {
            if (data.TryGetValue(name, out var value))
                return value;
            return null;
        }

        public void SetValue(string name, object value)
        {
            if (data.ContainsKey(name))
                data[name] = value;
        }

        public void AddDefaultValue(string name, object value)
        {
            if (!data.ContainsKey(name))
                data.Add(name, value);
        }
    }
}