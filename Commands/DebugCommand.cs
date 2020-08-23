using DirtBot.Services;
using DirtBot.Translation;
using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace DirtBot.Commands
{
    [Group("debug")]
    [RequireOwner]
    public class DebugCommand : ModuleBase<SocketCommandContext>
    {
        [Group("test")]
        public class Test : ModuleBase<SocketCommandContext>
        {
            [Command]
            public async Task DefaultTestCommand()
            {
                await ReplyAsync("Default test!");
            }

            [Command("command")]
            public async Task TestCommand()
            {
                await ReplyAsync("Test");
            }
        }

        [Command("set-loglevel")]
        public async Task SetLogLevel([Remainder] string level = null)
        {
            var eb = new EmbedBuilder()
                .WithTitle("Log Level");

            string possibleValues = "Possible values: `debug`, `info`, `warn`, `error`, `critical`";
            if (level is null)
                eb.Description = possibleValues;
            else
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

                if (logLevel == null)
                {
                    eb.Color = EmbedFactory.Error;
                    eb.Description = $"Unknown log level **'{level}'**\n{possibleValues}";
                }
                else
                {
                    Logger.GetLogger("Log Level").Important($"Log level set to {logLevel}");
                    Logger.SetLogLevel(logLevel.Value);
                    eb.Color = EmbedFactory.Success;
                    eb.Description = $"Set log level to {logLevel}";
                }
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

        [Command("error")]
        public Task ErrorCommand()
        {
            throw new Exception("Error!");
        }
    }
}