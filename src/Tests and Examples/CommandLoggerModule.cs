using DirtBot.Core;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using System;
using System.Linq;

namespace Tests_and_Examples
{
    public class CommandLoggerModule : Module
    {
        public override string Name => "logger";

        public override string DisplayName => "Command Logger";

        public CommandLoggerModule(IServiceProvider services) : base(services)
        {
            Log = new Logger("Command Logger", DirtBot.LogLevel);

            Client.GetCommandsNext().SetHelpFormatter<HelpCommand>();

            Client.GetCommandsNext().CommandErrored += async (e) =>
            {
                var errorEmbed = new DiscordEmbedBuilder()
                    .WithTitle("**An error occured**")
                    .WithColor(DiscordColor.Red);

                if (e.Exception is CommandNotFoundException)
                    errorEmbed.WithDescription("Command not found");
                else if (e.Exception is ArgumentException)
                    errorEmbed.WithDescription($"Could not find a command with those arguments\nSee `help {e.Context.Command.QualifiedName}` for help");
                else if (e.Exception is AggregateException)
                    errorEmbed.WithDescription("The command failed. See log for details");
                else if (e.Exception is ChecksFailedException ex)
                {
                    var checks = ex.FailedChecks;
                    do
                    {
                        // Source checks
                        if (checks.FirstOrDefault(x => x is RequireDirectMessageAttribute) is RequireDirectMessageAttribute)
                        {
                            errorEmbed.WithDescription("This command must be executed in DMs only.");
                            break;
                        }
                        if (ex.FailedChecks.FirstOrDefault(x => x is RequireGuildAttribute) is RequireGuildAttribute)
                        {
                            errorEmbed.WithDescription("This command must be executed in a guild.");
                            break;
                        }

                        // Permission checks
                        if (checks.FirstOrDefault(x => x is RequireBotPermissionsAttribute) is RequireBotPermissionsAttribute botPerms)
                            errorEmbed.AddField("I need one or more of these permissions", botPerms.Permissions.ToPermissionString());
                        if (checks.FirstOrDefault(x => x is RequireUserPermissionsAttribute) is RequireUserPermissionsAttribute userPerms)
                            errorEmbed.AddField("You need one or more of these permissions", userPerms.Permissions.ToPermissionString());

                        // Required roles
                        if (checks.FirstOrDefault(x => x is RequireRolesAttribute) is RequireRolesAttribute roles)
                            errorEmbed.AddField("You need one of these roles to do that", String.Join(", ", roles.RoleNames));

                        // Owner
                        if (checks.FirstOrDefault(x => x is RequireOwnerAttribute) is RequireOwnerAttribute owner)
                        {
                            errorEmbed.WithDescription("You must be my owner to do that!");
                            break;
                        }

                        // Nsfw
                        if (checks.FirstOrDefault(x => x is RequireNsfwAttribute) is RequireNsfwAttribute nsfw)
                        {
                            errorEmbed.WithDescription("This command needs to be executed in an NSFW channel. No idea why");
                            break;
                        }
                    } while (false);

                    if (errorEmbed.Fields.Count == 0 && String.IsNullOrEmpty(errorEmbed.Description))
                    {
                        errorEmbed.WithDescription("Some check failed here so I can't allow you to do that. Please report this to my owner.");
                        Log.Error($"A failed check was not handled. Failed Checks: {String.Join(", ", ex.FailedChecks)}");
                    }
                }
                else if (e.Exception != null)
                    errorEmbed.WithDescription("Internal error. See log for details");

                if (e.Exception != null)
                    await e.Context.RespondAsync(embed: errorEmbed.Build());
            };
        }
    }
}
