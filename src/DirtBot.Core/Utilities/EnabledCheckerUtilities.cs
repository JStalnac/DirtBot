using DSharpPlus.CommandsNext;

namespace DirtBot.Core.Utilities
{
    internal static class EnabledCheckerUtilities
    {
        public static bool IsEnabled(CommandGroup cmd, string[] disabledModules)
        {
            var log = new Logger("EnabledCheckerUtilities", LogLevel.Debug);
            log.Info($"cmd null: {cmd is null}");
            log.Info($"cmd Parent null: {cmd.Parent is null}");
            return false;
        }
    }
}
