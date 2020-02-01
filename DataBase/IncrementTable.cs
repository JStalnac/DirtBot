using System;
using System.Text;
using System.Collections.Generic;

namespace DirtBot.DataBase
{
    public class IncrementTable : LookupTable<dynamic, dynamic>
    {
        public override string FileName => "increment";
        public new string FullName => $"guilds/{Id}/{FileName}.{FileType}";
        public override bool RequiresTemplateFile => false;
        public ulong Id { get; private set; }

        Dictionary<dynamic, dynamic> increments => Table;

        public IncrementTable(ulong guildId)
        {
            Id = guildId;

            Dash.CMD.DashCMD.WriteImportant(FullName);

            if (!increments.ContainsKey("increment")) 
            {
                increments.Add("increment", 0);
                SaveData();
            }
        }
    }
}
