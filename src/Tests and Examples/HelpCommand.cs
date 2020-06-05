using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;

namespace Tests_and_Examples
{
    public class HelpCommand : BaseHelpFormatter
    {
        DiscordEmbedBuilder embed;

        public HelpCommand(CommandContext ctx) : base(ctx)
        {
            embed = new DiscordEmbedBuilder()
                .WithTitle("Help");
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            embed.AddField(command.QualifiedName, $"{command.Description}\n{String.Join(", ", command.Aliases)}");

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            // It's late ok
            foreach (var cmd in subcommands)
            {
                embed.AddField(cmd.QualifiedName, $"{cmd.Description}\n{String.Join(", ", cmd.Aliases)}");
            }

            return this;
        }

        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(embed: embed.Build());
        }
    }
}
