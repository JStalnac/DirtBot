using System;
using System.Collections.Generic;
using System.Text;

namespace DirtBot.Commands
{
    public interface IHasDataFile
    {
        string FileName { get; }
        CommandData[] Data { get; }
        IReadOnlyCollection<CommandData> DataFormat { get; }
    }
}
