using System;
using System.Collections.Generic;
using System.Text;

namespace DirtBot.DataBase
{
    public class GuildDataLookUp : LookupTable<ulong, object>
    {
        public ulong Id { get; private set;}
        public override string FileName => $"guild";

        public GuildDataLookUp(ulong id)
        {
            Id = id;
        }
    }
}
