using System;
using System.Text;
using System.Collections.Generic;
using Discord;

namespace DirtBot
{
    public class Emoji : IEmote
    {
        public ulong Id { get; private set; }
        public string Name { get; private set; }
        public bool IsAnimated { get; private set; }
        private Emote Emote;

        public Emoji(ulong id, string name, bool isAnimated)
        {
            Id = id;
            Name = name;
            IsAnimated = isAnimated;

            Emote = Emote.Parse($"<{(IsAnimated ? "a" : "")}:{Name}:{Id}>");
        }

        public override string ToString() => Emote.ToString();
    }
}
