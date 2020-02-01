using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace DirtBot.DataBase
{
    public class GuildLookup : LookupTable<ulong, GuildDataLookUp>
    {
        public override string FileName => "guilds";
        public new static Dictionary<ulong, GuildDataLookUp> Table;

        public GuildLookup()
        {
            EnsureStorageFile();
            LoadData();
        }

        public void LoadGuilds() 
        {
            foreach (var item in Table)
            {
                Dash.CMD.DashCMD.WriteStandard(item.Key.ToString());

                if (!Directory.Exists($"guilds/{item.Key.ToString()}"))
                {
                    Dash.CMD.DashCMD.WriteWarning($"Guild 'guilds/{item.Key.ToString()}' could not be found.");
                    item.Value.EnsureStorageDirectory();
                    item.Value.LoadData();
                }
            }
        }
    }
}
