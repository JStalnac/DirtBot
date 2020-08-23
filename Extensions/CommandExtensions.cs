using DirtBot.Attributes;
using DirtBot.Attributes.Preconditions;
using DirtBot.Services;
using DirtBot.Translation;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirtBot.Extensions
{
    public static class CommandExtensions
    {
        /// <summary>
        /// Gets the tags associated with the command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static string[] GetTags(this CommandInfo command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var tags = command.Attributes.FirstOrDefault(x => x is TagsAttribute);
            if (tags != null)
                return ((TagsAttribute)tags).Tags;
            else
                return Array.Empty<string>();
        }

        /// <summary>
        /// Creates a help embed for the command.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static async Task<Embed> CreateHelpEmbedAsync(this CommandInfo command, ICommandContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var ts = await TranslationManager.CreateFor(context.Channel);

            var eb = EmbedFactory.CreateNeutral()
                .WithTitle(ts.GetMessage("help:embed_title"))
                // Name
                .AddField(ts.GetMessage("help:field_name"), command.Aliases[0])
                // Description
                .AddField(x =>
                {
                    x.Name = ts.GetMessage("help:field_description");
                    if (String.IsNullOrEmpty(command.Summary))
                        x.Value = ts.GetMessage("help:no_description");
                    else
                        x.Value = ts.GetMessage(command.Summary);
                });

            // Remarks
            if (!String.IsNullOrEmpty(command.Remarks))
                eb.AddField(ts.GetMessage("help:field_remarks"), command.Remarks);

            // Categories
            if (command.Preconditions.FirstOrDefault(p => p is CategoriesAttribute) is CategoriesAttribute ca)
                eb.AddField(ts.GetMessage("help:field_categories"), String.Join(", ", ca.Categories.Select(x => Format.Code(x))));

            // Tags
            var tags = command.GetTags();
            if (tags.Any())
                eb.AddField(ts.GetMessage("help:field_tags"), String.Join(", ", tags.Select(t => Format.Code(t))));

            // Usage and aliases
            eb.AddField(ts.GetMessage("help:field_usage"), Format.Code(GetUsage(command)))
                .AddField(ts.GetMessage("help:field_aliases"), String.Join(", ", command.Aliases.Select(x => Format.Code(x))));

            // Parameters
            if (command.Parameters.Any())
            {
                eb.AddField(ts.GetMessage("help:field_parameters"), String.Join("\n", command.Parameters.Select(p =>
                    {
                        var sb = new StringBuilder();
                        sb.Append($"{Format.Code(p.Name)}: ");
                        if (String.IsNullOrEmpty(p.Summary))
                            sb.Append(ts.GetMessage("help:no_description"));
                        else
                            sb.Append(ts.GetMessage(p.Summary));
                        return sb.ToString();
                    })));
            }
            return eb.Build();
        }

        /// <summary>
        /// Gets the usage for the command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static string GetUsage(this CommandInfo command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var sb = new StringBuilder();
            sb.Append(command.Aliases[0]);

            foreach (var p in command.Parameters)
            {
                sb.Append(" ");

                if (p.IsOptional)
                    sb.Append($"[{p.Name}]");
                else
                    sb.Append($"<{p.Name}>");
            }

            if (command.HasVarArgs)
                sb.Append(" ...");

            return sb.ToString();
        }
    }
}
