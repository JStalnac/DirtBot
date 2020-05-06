using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;
using DirtBot.Core;

namespace DirtBot.Internal
{
    public class PrefixModule : Module
    {
        public override string DisplayName => "Prefix";

        public override string Name => "prefix";

        public PrefixModule(IServiceProvider services) : base(services)
        {
            Log = new Logger("Prefix Manager", DirtBot.LogLevel);
            
            Client.Ready += async (e) => 
            {
                Log.Info("I'm Ready!");
            };

            var config = GetConfiguration();
            config.AddDefaultValue("defaultPrefix", "dirt ");
            config.Save();
            Log.Info($"Default prefix: '{config.GetValue("defaultPrefix")}'");
        }

        [Command("hello")]
        public async Task Hello(CommandContext ctx)
        {
            await ctx.RespondAsync("Hello World! 👋");
        }
    }
}
