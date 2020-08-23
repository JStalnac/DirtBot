using DirtBot.Services;
using Discord.Commands;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DirtBot.Attributes.Preconditions
{
    /// <summary>
    /// Used to determine what categories a command is in and to disable them. If you want to add tags to the help command please use <see cref="TagsAttribute"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class CategoriesAttribute : PreconditionAttribute
    {
        public string[] Categories { get; }

        public CategoriesAttribute(params string[] categories)
        {
            if (categories.Length == 0)
                throw new ArgumentNullException(nameof(categories));
            if (categories.Any(x => String.IsNullOrEmpty(x?.Trim())))
                throw new ArgumentException("Tags must not be null or empty", nameof(categories));
            Categories = categories;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild is null)
                return PreconditionResult.FromSuccess();
            // In a guild
            var cm = Dirtbot.Services.GetRequiredService<CategoryManagerService>();
            var globalDisabled = await cm.GetDisabledCategoriesGlobalAsync();

            if (globalDisabled.Any(x => Categories.Any(c => x == c)))
                return PreconditionResult.FromError("errors:module_disabled_global");

            var disabled = await cm.GetDisabledCategoriesAsync(context.Guild.Id);

            if (disabled.Any(x => Categories.Any(c => x == c)))
                return PreconditionResult.FromError("errors:module_disabled");
            return PreconditionResult.FromSuccess();
        }
    }
}
