using System.Collections.Generic;

namespace DirtBot.Commands
{
    public interface IHasDataFile
    {
        string StaticStorage { get; }
        string GuildStorage { get; }
        List<ModuleData> DefaultStaticData { get; }
        List<ModuleData> DefaultData { get; }
    }
}
