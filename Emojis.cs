using System.Collections.Generic;

namespace DirtBot
{
    /// <summary>
    /// List of all the emojies
    /// </summary>
    public class Emojis
    {
        Dictionary<string, Emoji> emotes = new Dictionary<string, Emoji>();

        public Emojis()
        {
            foreach (Emoji emoji in Config.Emotes)
            {
                if (GetEmoji(emoji.Name) is null) emotes.Add(emoji.Name, emoji);
            }
        }

        public Emoji this[string name]
        {
            get => GetEmoji(name);
        }

        public Emoji GetEmoji(string name)
        {
            try
            {
                return emotes[name];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public Emoji DirtDontPingMe { get => emotes["dirtdontpingme"]; }
        public Emoji DirtBlobHyperHyper { get => emotes["dirtblobhyperhyper"]; }
    }
}
