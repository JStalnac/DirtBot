using System;

namespace DirtBot.DataBase
{
    public class GuildLookUp<TKey, TValue> : LookupTable<TKey, TValue>
    {
        public override string FileName => throw new NotImplementedException();
    }
}
