using DirtBot.Core;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace DirtBot.Internal
{
    public class PrefixModule : BaseCommandModule
    {
        string defaultPrefix = "dirt ";

        public PrefixModule(IServiceProvider services)
        {
            var bot = services.GetRequiredService<Core.DirtBot>();

            var config = Configuration.LoadConfiguration("modules/config/prefix/config.yml");
            config.AddDefaultValue("defaultPrefix", "dirt ");
            config.Save();
            new Logger("Prefix Manager", bot.LogLevel).Info($"Default prefix: '{config.GetValue("defaultPrefix")}'");
            defaultPrefix = config.GetValue("defaultPrefix") as string;
        }

        [Command("prefix")]
        [Description("Gets or sets the server's prefix")]
        public async Task Prefix(CommandContext ctx, [RemainingText, Description("The new prefix. Leave empty for the current prefix")] string prefix = null)
        {
            if (prefix is null)
            {
                if (ctx.Channel.IsPrivate)
                {
                    await ctx.RespondAsync($"My prefix is `{defaultPrefix}`");
                    return;
                }

                var db = ctx.GetStorage("prefix") as IDatabaseAsync;
                string guildPrefix = await db.StringGetAsync("prefix");
                guildPrefix = guildPrefix == null ? defaultPrefix : guildPrefix;

                await ctx.RespondAsync($"My prefix is `{guildPrefix}`");
            }
            else
            {
                if (ctx.Channel.IsPrivate)
                {
                    await ctx.RespondAsync("You cannot change the prefix is private messages!");
                    return;
                }

                var db = ctx.GetStorage("prefix") as IDatabaseAsync;
                await db.StringSetAsync("prefix", prefix, flags: CommandFlags.FireAndForget);
                await ctx.RespondAsync($"The server's prefix is now `{prefix}`");
            }
        }

        [Command("prefixwithspace")]
        public async Task PrefixWithSpace(CommandContext ctx, [RemainingText] string prefix)
        {
            if (String.IsNullOrEmpty(prefix))
                return;
            await Prefix(ctx, $"{prefix} ");
        }
    }
}
