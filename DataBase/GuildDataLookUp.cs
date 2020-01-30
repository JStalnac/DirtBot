using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace DirtBot.DataBase
{
    public class GuildDataLookUp : LookupTable<ulong, object>
    {
        public ulong Id { get; private set; }
        public override string FileName => Id.ToString();

        public new string FullName => $"guilds/{Id}/{FileName}.{FILETYPE}";

        public GuildDataLookUp(ulong guildId)
        {
            Id = guildId;
        }

        public void EnsureStorageDirectory() 
        {
            if (!Directory.Exists($"guilds/{Id}")) 
            {
                Directory.CreateDirectory($"guilds/{Id}");
            }
        }


        // TODO: Load the data for the lookups in the guild folders.
        // TODO: Dictionary for LookUps. Bans etc
    }
}
