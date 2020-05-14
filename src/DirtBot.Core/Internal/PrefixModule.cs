using DirtBot.Core;
using DirtBot.Core.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace DirtBot.Internal
{
    [Required]
    public class PrefixModule : CommandModule
    {
        public override string DisplayName => "Prefix";

        public override string Name => "prefix";

        string defaultPrefix = "dirt ";

        public PrefixModule(IServiceProvider services) : base(services)
        {
            Log = new Logger("Prefix Manager", DirtBot.LogLevel);

            var config = GetConfiguration();
            config.AddDefaultValue("defaultPrefix", "dirt ");
            config.Save();
            Log.Info($"Default prefix: '{config.GetValue("defaultPrefix")}'");
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

                var db = GetStorage(ctx.Guild) as IDatabaseAsync;
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

                var db = GetStorage(ctx.Guild) as IDatabaseAsync;
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

        [Command("hello")]
        public async Task Hello(CommandContext ctx)
        {
            await ctx.RespondAsync("Hello World! 👋");
        }
    }
}
