using DSharpPlus.CommandsNext.Attributes;
using System;

namespace DirtBot.Core.Internal
{
    [Hidden]
    [RequireOwner]
    [Group("owner")]
    public class OwnerModule : CommandModule
    {
        public override string Name => "owner";

        public override string DisplayName => "Owner Module";

        public OwnerModule(IServiceProvider services) : base(services) { }

        /*
        [Hidden]
        [Group("module")]
        public class ModuleManagerModule : CommandModule
        {
            public override string Name => "modulemanager";

            public override string DisplayName => "Module Manager";

            public ModuleManagerModule(IServiceProvider services) : base(services)
            {
                Log = new Logger("Module Manager");
                Client.Ready += async (e) =>
                {
                    Log.Info($"Current active modules:\n{String.Join("\n", Modules)}");
                };
            }

            [Command("disable")]
            [Description("Disable a module for **all guilds** WIP")]
            public async Task DisableModule(CommandContext ctx, [RemainingText, Description("The internal name of the module. List with module list")]string moduleName)
            {
                await ctx.RespondAsync("Hello!");

                if (moduleName == Name)
                {
                    await ctx.RespondAsync("You cannot disable this module.");
                    return;
                }
                else if (moduleName == "owner")
                {
                    await ctx.RespondAsync("You cannot disable the owner module.");
                    return;
                }

                var m = Modules.Select(x => x.Name == moduleName);
                if (m is null)
                {
                    await ctx.RespondAsync($"No module found with the internal name '{moduleName}'");
                    return;
                }

                var db = Redis.GetDatabase(0) as IDatabaseAsync;
                var disabledModules = await db.SetMembersAsync("disabled_modules");
                if (disabledModules.Length != 0)
                {
                    if (disabledModules.Contains(moduleName))
                    {
                        await ctx.RespondAsync("That module is already disabled!");
                        return;
                    }
                }

                await db.SetAddAsync("disabled_modules", moduleName, flags: CommandFlags.FireAndForget);
                ctx.RespondAsync($"Disabled module `{moduleName}`");
            }

            [Command("enable")]
            [Description("Enable a module for **all guilds** WIP")]
            public async Task EnableModule(CommandContext ctx, [RemainingText, Description("The internal name of the module. List with module list")]string moduleName)
            {
                var m = Modules.Select(x => x.Name == moduleName);
                if (m is null)
                {
                    await ctx.RespondAsync($"Could not find a loaded module with the internal name '{moduleName}'");
                    return;
                }

                var db = Redis.GetDatabase(0) as IDatabaseAsync;
                db.SetRemoveAsync("disabled_modules", moduleName, flags: CommandFlags.FireAndForget);
            }

            [Command("list")]
            [Description("Lists all the modules.")]
            public async Task ListModules(CommandContext ctx)
            {
                var embed = new DiscordEmbedBuilder();
                embed.WithTitle("List of modules");
                embed.AddField("Name", "Internal name");

                foreach (var module in Modules)
                {
                    embed.AddField(module.DisplayName, module.Name, true);
                }
                ctx.RespondAsync(embed: embed.Build());
            }
        }
        */
    }
}
