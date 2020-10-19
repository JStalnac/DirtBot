using DirtBot.Utilities;
using DirtBot.Translation;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirtBot.Services
{
    public enum SearchContext
    {
        Commands = 1 << 2,
        Tags = 2 << 2
    }

    public class HelpProviderService
    {
        private readonly CommandService cs;
        private readonly IServiceProvider services;

        public HelpProviderService(IServiceProvider services, CommandService commandService)
        {
            cs = commandService;
            this.services = services;
        }

        public async Task<Embed> HelpAsync(ICommandContext context, string query, SearchContext searchContext)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (query is null)
                throw new ArgumentNullException(nameof(query));

            var commands = (await SearchCommands(context, query, searchContext)).ToList();

            if (commands.Count == 0)
            {
                var ts = await TranslationManager.CreateFor(context.Channel);
                return EmbedFactory.CreateNeutral()
                    .WithTitle(ts.GetMessage("help:embed_title"))
                    .WithDescription(ts.GetMessage("help:command_not_found"))
                    .Build();
            }

            // NOTE: This system breaks when there are two commands with the same alias. Avoid using same aliases
            if (commands.Count > 1)
            {
                var ts = await TranslationManager.CreateFor(context.Channel);
                return EmbedFactory.CreateNeutral()
                    .WithTitle(ts.GetMessage("help:embed_title"))
                    .WithDescription($"{ts.GetMessage("help:multiple_matches")}\n{String.Join("\n", commands.Select(x => Format.Code(x.GetUsage())))}")
                    .Build();
            }
            else
                return await commands[0].CreateHelpEmbedAsync(context);
        }

        public async Task<IEnumerable<CommandInfo>> SearchCommands(ICommandContext context, string query, SearchContext searchContext)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (query is null)
                throw new ArgumentNullException(nameof(query));

            var ec = await cs.GetExecutableCommandsAsync(context, services);

            if (searchContext.HasFlag(SearchContext.Commands))
            {
                // Direct equal
                var result = ec.Where(x => x.Aliases.Any(a => a.Equals(query, StringComparison.OrdinalIgnoreCase))).ToList();
                if (result.Count > 0)
                    return result;

                // Contains
                result = ec.Where(x => x.Aliases.Any(a => a.Contains(query, StringComparison.OrdinalIgnoreCase))).ToList();
                if (result.Count > 0)
                    return result;
            }
            if (searchContext.HasFlag(SearchContext.Tags))
            {
                // Any tags
                var result = ec.Where(x => x.GetTags().Any(y => y.Equals(query, StringComparison.OrdinalIgnoreCase))).ToList();
                if (result.Count > 0)
                    return result;
            }

            return Array.Empty<CommandInfo>();
        }
    }
}
