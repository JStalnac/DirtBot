using System;

namespace DirtBot.Services.Options
{
    public class PrefixManagerOptions
    {
        public string DefaultPrefix
        {
            get => defaultPrefix;
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException();
                defaultPrefix = value;
            }
        }
        private string defaultPrefix;
    }
}
