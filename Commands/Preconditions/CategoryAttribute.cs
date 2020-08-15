using DirtBot.Services;
using Discord.Commands;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DirtBot.Commands.Preconditions
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class CategoryAttribute : PreconditionAttribute
    {
        public string Category { get; }

        public CategoryAttribute(string category)
        {
            Category = category;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild is null)
                return PreconditionResult.FromSuccess();
            // In a guild
            var cm = Dirtbot.Services.GetRequiredService<CategoryManagerService>();
            var globalDisabled = await cm.GetDisabledCategoriesGlobalAsync();

            if (globalDisabled.Any(x => x == Category))
                return PreconditionResult.FromError("errors:module_disabled_global");

            var disabled = await cm.GetDisabledCategoriesAsync(context.Guild.Id);

            if (disabled.Any(x => x == Category))
                return PreconditionResult.FromError("errors:module_disabled");
            return PreconditionResult.FromSuccess();
        }
    }
}
