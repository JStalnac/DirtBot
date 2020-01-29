using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace DirtBot.DataBase
{
    public class GuildLookUp : LookupTable<ulong, object>
    {
        public override string FileName => "guilds";

        public GuildLookUp()
        {
            LoadData();

            foreach (var item in Table)
            {
                if (!Directory.Exists($"guilds/{item.Key.ToString()}")) 
                {
                    Dash.CMD.DashCMD.WriteWarning($"Guild 'guilds/{item.Key.ToString()}' could not be found.");
                    Directory.CreateDirectory($"guilds/{item.Key.ToString()}");
                }
            }
        }
    }

    // TODO: Make another class that implements LookUpTable as : LookUpTable<string, [some type]>
    // TODO: Replace LookUpTable<ulong, object>
    // TODO: Load the data for the lookups in the guild folders.
}
