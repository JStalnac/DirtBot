using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DirtBot.Services
{
    public class TranslationService : ServiceBase
    {
        private static bool loading;

        public async Task LoadTranslations()
        {
            
        }

        public string GetMessage(string path)
        {
            if (loading)
                return path;

            if (String.IsNullOrEmpty(path?.Trim()))
                throw new ArgumentNullException(nameof(path));
            // This regex will sanitize the input for us.
            var match = Regex.Match(path, @"^([\w+\/]+)(?<!\/):(\w+)$");
            if (!match.Success)
                throw new FormatException("Invalid path format");

            // Get the key and split the path
            string key = match.Groups[1].Value;
            string[] pathParts = match.Groups[0].Value.Split('/');
            // Regex not smart enough
            if (pathParts.Any(x => x == ""))
                throw new FormatException("Invalid path format. Empty path part");

            // TODO: Get message
            return path;
        }
    }
}