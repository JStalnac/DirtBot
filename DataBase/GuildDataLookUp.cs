using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Dash.CMD;

namespace DirtBot.DataBase
{
    public class GuildDataLookUp : LookupTable<string, LookupTable<dynamic, dynamic>>
    {
        public ulong Id { get; private set; }
        public override string FileName => Id.ToString();

        Dictionary<string, LookupTable<dynamic, dynamic>> Lookups => Table;

        public new string FullName => $"guilds/{Id}/{FileName}.{FileType}";

        public GuildDataLookUp(ulong guildId)
        {
            if (guildId == 0) 
            {
                throw new ArgumentNullException("guildId");
            }
            Id = guildId;

            // Make a more efficient way to do this in the future...
            Lookups.Add("increment", new IncrementTable(guildId));

            DashCMD.WriteStandard($"Ensuring data directory for {guildId}");
            EnsureStorageDirectory();
        }

        public new void LoadData()
        {
            foreach (ILookup lookup in Lookups.Values)
            {
                lookup.FullName = $"guilds/{Id}/{lookup.FileName}.{lookup.FileType}";
                lookup.EnsureStorageFile();
                lookup.LoadData();
            }
        }

        public void EnsureStorageDirectory() 
        {
            if (!Directory.Exists($"guilds/{Id}"))
                Directory.CreateDirectory($"guilds/{Id}");
        }

        // TODO: Load the data for the lookups in the guild folders.    - Done -- Not tested
        // TODO: Dictionary for Lookups. Bans etc                       - Done -- Not tested
        // TODO: Lookup data saving...
    }
}
