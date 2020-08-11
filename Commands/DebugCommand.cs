using System;
using System.Threading.Tasks;
using DirtBot.Translation;
using Discord;
using Discord.Commands;

namespace DirtBot.Commands
{
    [Group("debug")]
    public class DebugCommand : ModuleBase<SocketCommandContext>
    {
        [Command("set-loglevel")]
        [RequireOwner]
        public async Task SetLogLevel([Remainder]string level)
        {
            LogLevel? logLevel = null;
            switch (level)
            {
                case "debug":
                    logLevel = LogLevel.Debug;
                    break;
                case "info":
                    logLevel = LogLevel.Info;
                    break;
                case "warn":
                    logLevel = LogLevel.Warning;
                    break;
                case "error":
                    logLevel = LogLevel.Error;
                    break;
                case "critical":
                    logLevel = LogLevel.Critical;
                    break;
            }

            var eb = new EmbedBuilder();
            eb.Title = "Log Level";
            if (logLevel == null)
            {
                eb.Color = new Color(0xff0000);
                eb.Description = $"Unknown log level **'{level}'**\nPossible values: `debug`, `info`, `warn`, `error`, `critical`";
            }
            else
            {
                Logger.GetLogger("Log Level").Important($"Log level set to {logLevel}");
                Logger.SetLogLevel(logLevel.Value);
                eb.Color = new Color(0x00ff00);
                eb.Description = $"Set log level to {logLevel}";
            }

            await ReplyAsync(embed: eb.Build());
        }

        [Command("reload-translations")]
        public async Task ReloadTranslations()
        {
            await ReplyAsync("Reloading...");
            try
            {
                await TranslationManager.LoadTranslations();
            }
            catch (Exception e)
            {
                await ReplyAsync($"Reloaded with exception: ```{e}```");
                Logger.GetLogger(this).Warning("Failed to load translations:", e);
                return;
            }
            await ReplyAsync("Reloaded! Check the log for possible warnings");
        }
    }
}