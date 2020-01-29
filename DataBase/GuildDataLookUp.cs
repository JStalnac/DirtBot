using System;
using System.Collections.Generic;
using System.Text;

namespace DirtBot.DataBase
{
    public class GuildDataLookUp : LookupTable<ulong, object>
    {
        public override string FileName => throw new Exception();
    }
}
