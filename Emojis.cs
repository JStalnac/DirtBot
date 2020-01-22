using System.Collections.Generic;
using Discord;

namespace DirtBot
{
    /// <summary>
    /// List of all the emojies
    /// </summary>
    public class Emojis
    {
        Dictionary<string, Emote> emotes = new Dictionary<string, Emote>();

        public Emojis()
        {
            AddEmoji("<:dirtdontpingme:634748801617231892>");
            AddEmoji("<:dirtblobhyperhyper:661269834805542933>");
        }

        public void AddEmoji(string id) 
        {
            Emote emote = Emote.Parse(id);
            emotes.Add(emote.Name, emote);
        }

        public Emote GetEmote(string name) 
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

        public Emote DirtDontPingMe { get => emotes["dirtdontpingme"]; }
        public Emote DirtBlobHyperHyper { get => emotes["dirtblobhyperhyper"]; }
        
    }
}
