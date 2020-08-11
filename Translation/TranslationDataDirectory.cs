using System.Collections.Generic;

namespace DirtBot.Translation
{
    internal class TranslationDataDirectory
    {
        public Dictionary<string, TranslationDataDirectory> Directories { get; }

        public Dictionary<string, Dictionary<string, IEnumerable<string>>> Data { get; }

        public TranslationDataDirectory(Dictionary<string,Dictionary<string, IEnumerable<string>>> data, Dictionary<string, TranslationDataDirectory> directories)
        {
            Data = data ?? new Dictionary<string, Dictionary<string, IEnumerable<string>>>();
            Directories = directories ?? new Dictionary<string, TranslationDataDirectory>();
        }
    }
}