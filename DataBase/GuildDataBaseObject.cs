using System;
using System.Collections.Generic;
using System.Text;

namespace DirtBot.DataBase
{
    internal class GuildDataBaseObject
    {
        public long Id { get; private set; }
        public string DisplayName { get; private set; }
        public string Prefix { get; private set; }
    }
}
