using DirtBot.Database;
using DirtBot.Database.Models;
using DirtBot.Utilities;
using DirtBot.Logging;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace DirtBot.Translation
{
    public class TranslationManager
    {
        private static bool loading;
        private static Dictionary<CultureInfo, TranslationDataDirectory> Data { get; set; } = new Dictionary<CultureInfo, TranslationDataDirectory>();
        private static readonly Random random = new Random();
        private static readonly Logger log = Logger.GetLogger<TranslationManager>();
        private static CultureInfo DefaultLanguage { get; } = new CultureInfo("en");
        
        private TranslationDataDirectory TranslationData { get; }
        private bool Default { get; }

        private TranslationManager(CultureInfo language)
        {
            if (language is null)
                throw new ArgumentNullException(nameof(language));

            // Because of this implementation TranslationManagers shouldn't be stored static because the language may update
            if (Data.TryGetValue(language, out var td))
                TranslationData = td;
            else if (Data.TryGetValue(DefaultLanguage, out var dd))
                TranslationData = dd;

            // else The language may be null but then everything is broken
            Default = language == DefaultLanguage;
        }

        /// <summary>
        /// Creates a new translation instance for the channel. Null channel returns the default language
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static async Task<TranslationManager> CreateFor(IChannel channel)
        {
            ulong id = GetId(channel);
            var lang = await GetLanguageAsync(id);
            return new TranslationManager(lang);
        }

        /// <summary>
        /// Gets a message translated message for a user or a guild. Id 0 returns the default message in en-us.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetMessage(string path)
        {
            if (loading)
                return path;

            if (String.IsNullOrEmpty(path?.Trim()))
                throw new ArgumentNullException(nameof(path));
            // This smart regex will sanitize the input for us.
            var match = Regex.Match(path, @"^([\w+\/]+)(?<!\/):(\w+)$");
            if (!match.Success)
                throw new FormatException("Invalid path format");

            // Get the key and split the path
            string key = match.Groups[2].Value;
            var pathParts = match.Groups[1].Value.Split('/');
            // Regex not smart enough
            if (pathParts.Any(x => x == ""))
                throw new FormatException("Invalid path format. Empty path part");

            IEnumerable<string> result = null;

            // Get message
            result = SearchMessage(TranslationData, pathParts, key);

            // Get message for default language if not found
            if (result is null && !Default && Data.TryGetValue(DefaultLanguage, out var d2))
            {
                result = SearchMessage(d2, pathParts, key);
                if (result != null)
                    log.Warning($"Had to fall to default message for key {path}");
            }

            if (result != null)
            {
                // Choose one if there are multiple
                var l = result.ToList();
                if (l.Count == 1)
                    return l[0];
                return l[random.Next(0, l.Count)];
            }
            else
                log.Warning($"No message found for key {path}");

            return path;
        }

        /// <summary>
        /// Searches for a message in a <see cref="TranslationDataDirectory"/>
        /// </summary>
        /// <param name="td"></param>
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private IEnumerable<string> SearchMessage(TranslationDataDirectory td, string[] path, string key)
        {
            if (td is null) throw new ArgumentNullException(nameof(td));
            if (path is null) throw new ArgumentNullException(nameof(path));
            if (String.IsNullOrEmpty(key?.Trim())) throw new ArgumentNullException(nameof(key));

            var dir = td;

            for (int i = 0; i < path.Length; i++)
            {
                // If this is the last part of the path
                if (i == path.Length - 1)
                {
                    // Get file
                    if (dir.Data.TryGetValue(path[i], out var d))
                    {
                        // Check key
                        if (d.TryGetValue(key, out var value)) return value;
                    }
                }
                // Not the last part
                else if (dir.Directories.TryGetValue(path[i], out var d))
                {
                    // We have a path that continues with this part
                    dir = d;
                    continue;
                }
            }
            return null;
        }

        public static bool HasLanguage(CultureInfo cultureInfo)
        {
            if (cultureInfo is null)
                return false;
            return Data.ContainsKey(cultureInfo);
        }

        /// <summary>
        /// Loads the translations from the lang/ directory
        /// </summary>
        /// <returns></returns>
        public static async Task LoadTranslations()
        {
            // Don't
            if (loading)
                return;
            loading = true;
            // Reset
            Data = new Dictionary<CultureInfo, TranslationDataDirectory>();

            // If there are no translation, don't load them
            if (!Directory.Exists("lang"))
            {
                log.Warning("Translation directory 'lang' not found");
                return;
            }

            var loads = new Dictionary<CultureInfo, Task<TranslationDataDirectory>>();
            foreach (string langDir in Directory.EnumerateDirectories("lang"))
            {
                // Get directory name
                CultureInfo ci;
                var di = new DirectoryInfo(langDir);
                try
                {
                    ci = new CultureInfo(di.Name);
                    // Only the language codes are allowed because of the implementation for the database and language choosing.
                    if (ci.TwoLetterISOLanguageName != di.Name)
                        // Not really but easier :)
                        throw new CultureNotFoundException();
                }
                catch (CultureNotFoundException)
                {
                    log.Warning($"Directory '{di.Name}' is not a valid language code.");
                    continue;
                }

                // Load data from this directory
                loads.Add(ci, LoadPath(di));
            }

            // Finish loading
            foreach (var kvp in loads)
                Data.Add(kvp.Key, await kvp.Value);
            loading = false;
        }

        /// <summary>
        /// Loads all the files and subdirectories from a directory
        /// </summary>
        /// <param name="di"></param>
        /// <returns></returns>
        private static async Task<TranslationDataDirectory> LoadPath(DirectoryInfo di)
        {
            // Collecting data here
            var directories = new Dictionary<string, TranslationDataDirectory>();
            var files = new Dictionary<string, Dictionary<string, IEnumerable<string>>>();
            // Tasks
            var directoryLoads = new Dictionary<DirectoryInfo, Task<TranslationDataDirectory>>();
            var fileLoads = new Dictionary<FileInfo, Task<string>>();

            // Trying to load directory contents
            IEnumerable<DirectoryInfo> directoryInfos;
            IEnumerable<FileInfo> fileInfos;
            try
            {
                directoryInfos = di.EnumerateDirectories();
                fileInfos = di.EnumerateFiles("*.yml");
            }
            catch (SecurityException)
            {
                // No permission
                log.Warning($"Failed to load contents in directory {di.FullName}: No permission: Skipping");
                return new TranslationDataDirectory(null, null);
            }

            // Load subdirectories
            log.Debug($"{di.FullName}: Loading directories");
            foreach (var directory in directoryInfos)
            {
                // Load the directory asynchronously
                log.Debug($"{di.FullName}: Loading directory {directory.FullName}");
                directoryLoads.Add(directory, LoadPath(directory));
            }

            // Load files from this directory.
            log.Debug($"{di.FullName}: Loading files");
            foreach (var file in fileInfos)
            {
                // Start loading files
                log.Debug($"{di.FullName}: Loading file {file.FullName}");
                fileLoads.Add(file, File.ReadAllTextAsync(file.FullName));
            }

            log.Debug($"{di.FullName}: Parsing and waiting files");
            // Load the files here as they will most likely take less to load.
            var d = new Deserializer();
            foreach (var kvp in fileLoads)
            {
                try
                {
                    // Deserialize the yaml data
                    log.Debug($"{di.FullName}: Parsing file {kvp.Key.FullName}");
                    var data = d.Deserialize<Dictionary<string, object>>(await kvp.Value)
                               ?? new Dictionary<string, object>();
                    // Checking the values
                    var result = new Dictionary<string, IEnumerable<string>>();
                    foreach (var dataPair in data)
                    {
                        // Single key
                        if (dataPair.Value is string)
                        {
                            result.Add(dataPair.Key, new[] { (string)dataPair.Value });
                            continue;
                        }
                        // Collection
                        if (dataPair.Value is IEnumerable<object> e)
                        {
                            // Process the collection
                            var r = new List<string>();
                            bool valid = true;
                            foreach (object o in e)
                            {
                                if (o is string)
                                    r.Add((string)o);
                                else
                                {
                                    valid = false;
                                    break;
                                }
                            }

                            if (valid)
                            {
                                result.Add(dataPair.Key, r);
                                continue;
                            }
                        }
                        // Invalid
                        log.Warning(
                            $"Invalid key: {dataPair.Key} in file {kvp.Key.FullName}: Must be string or collection");
                    }

                    files.Add(Path.GetFileNameWithoutExtension(kvp.Key.Name), result);
                }
                catch (IOException e)
                {
                    log.Warning($"Failed to load data from file {kvp.Key.FullName}:", e);
                }
                catch (SecurityException)
                {
                    log.Warning($"Failed to load data from file {kvp.Key.FullName}: No permission");
                }
                catch (YamlException e)
                {
                    log.Warning($"Failed to load data from file {kvp.Key.FullName}: Parse failed: {e.Message}");
                }
            }

            log.Debug($"{di.FullName}: Waiting for directories");
            // Now loading all directories
            foreach (var kvp in directoryLoads)
            {
                log.Debug($"{di.FullName}: Waiting for directory {kvp.Key.FullName}");
                directories.Add(kvp.Key.Name, await kvp.Value);
            }

            // Return the result
            log.Debug($"{di.FullName}: Done");
            return new TranslationDataDirectory(files, directories);
        }

        /// <summary>
        /// Gets the language set for a user or a guild. If id is zero return the English language.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<CultureInfo> GetLanguageAsync(ulong id)
        {
            if (id == 0)
                return DefaultLanguage;

            try
            {
                // Get cached data from Redis
                var rdb = Program.Redis.GetDatabase(0);
                string language = await rdb.StringGetAsync($"languages:{id}");

                // Key stored in Redis
                if (language != null)
                    return new CultureInfo(language);

                // Get the prefix from database
                using (var db = Program.Services.GetRequiredService<DatabaseContext>())
                    language = (await AsyncEnumerable.FirstOrDefaultAsync(db.Languages, p => p.Id == id))?.Language;
                language ??= "en";
                var result = new CultureInfo(language);
                CacheLanguage(id, result).Release();
                return result;
            }
            catch (CultureNotFoundException)
            {
                return new CultureInfo("en");
            }
        }

        /// <summary>
        /// Sets the language used for a user or a guild.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static async Task SetLanguageAsync(ulong id, CultureInfo language)
        {
            if (language is null)
                throw new ArgumentNullException(nameof(language));

            using (var db = Program.Services.GetRequiredService<DatabaseContext>())
            {
                var data = await AsyncEnumerable.FirstOrDefaultAsync(db.Languages, x => x.Id == id);
                if (data is null)
                    await db.AddAsync(new LanguageData
                    {
                        Id = id,
                        Language = language.TwoLetterISOLanguageName
                    });
                else
                {
                    data.Language = language.TwoLetterISOLanguageName;
                    db.Entry(data).State = EntityState.Modified;
                }

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    Logger.GetLogger<TranslationManager>().Warning($"Failed to update preferred language for {id}. Language: '{language.TwoLetterISOLanguageName}'");
                }
            }

            // Update the cache
            CacheLanguage(id, language).Release();
        }

        private static async Task CacheLanguage(ulong id, CultureInfo language)
        {
            // Cache the prefix for the guild in Redis.
            await Program.Redis.GetDatabase(0).StringSetAsync($"languages:{id}", language.TwoLetterISOLanguageName, TimeSpan.FromMinutes(30),
                flags: CommandFlags.FireAndForget);
        }

        /// <summary>
        /// Gets the id that can be used for getting a translation from the channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static ulong GetId(IChannel channel)
        {
            if (channel is IDMChannel dm)
                return dm.Recipient.Id;
            if (channel is IPrivateChannel pm)
                return pm.Id;
            if (channel is IGuildChannel gc)
                return gc.GuildId;
            return 0;
        }
    }
}