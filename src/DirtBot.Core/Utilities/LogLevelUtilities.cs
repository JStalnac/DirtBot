using System;

namespace DirtBot.Core.Utilities
{
    public static class LogLevelUtilities
    {
        public static DSharpPlus.LogLevel DirtBotToDSharpPlus(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return DSharpPlus.LogLevel.Debug;
                case LogLevel.Info:
                    return DSharpPlus.LogLevel.Info;
                case LogLevel.Warning:
                    return DSharpPlus.LogLevel.Warning;
                case LogLevel.Error:
                    return DSharpPlus.LogLevel.Error;
                case LogLevel.Critical:
                    return DSharpPlus.LogLevel.Critical;
                default:
                    // Not going to happen but
                    throw new InvalidOperationException($"Could not convert {nameof(LogLevel)} to {nameof(DSharpPlus.LogLevel)}!");
            }
        }

        public static LogLevel DSharpPlusToDirtBot(DSharpPlus.LogLevel level)
        {
            switch (level)
            {
                case DSharpPlus.LogLevel.Debug:
                    return LogLevel.Debug;
                case DSharpPlus.LogLevel.Info:
                    return LogLevel.Info;
                case DSharpPlus.LogLevel.Warning:
                    return LogLevel.Warning;
                case DSharpPlus.LogLevel.Error:
                    return LogLevel.Error;
                case DSharpPlus.LogLevel.Critical:
                    return LogLevel.Critical;
                default:
                    // Not going to happen but
                    throw new InvalidOperationException($"Could not convert {nameof(DSharpPlus.LogLevel)} to {nameof(LogLevel)}!");
            }
        }
    }
}
