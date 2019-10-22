using System;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace DirtBot
{
    /// <summary>
    /// List of all the dirtemojies
    /// </summary>
    public class Emojis
    {
        Dictionary<string, Emote> emotes = new Dictionary<string, Emote>();

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

        public Emote DirtDontPingMe 
        {
            get { return emotes["dirtdontpingme"]; }
        }
    }
}
