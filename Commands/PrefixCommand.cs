using DirtBot.Attributes;
using DirtBot.Extensions;
using DirtBot.Services;
using DirtBot.Translation;
using Discord;
using Discord.Commands;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DirtBot.Commands
{
    public class PrefixCommand : ModuleBase<SocketCommandContext>
    {
        private readonly PrefixManagerService pm;

        public PrefixCommand(PrefixManagerService prefixManager)
        {
            pm = prefixManager;
        }

        [Command("prefix")]
        [Tags("settings", "system")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "Perms", ErrorMessage = "errors/user:permission_administrator", NotAGuildErrorMessage = "errors:not_a_guild")]
        [RequireUserPermission(GuildPermission.ManageGuild, Group = "Perms", ErrorMessage = "errors/user:permission_manage_guild", NotAGuildErrorMessage = "errors:not_a_guild")]
        public async Task Prefix([Summary("commands/prefix:parameter_prefix")][Remainder] string prefix = null)
        {
            // Prepare reply
            EmbedBuilder eb;
            //var eb = new EmbedBuilder();
            var reply = new StringBuilder();
            var ts = await TranslationManager.CreateFor(Context.Channel);

            // Get the prefix
            if (prefix is null)
            {
                // Get prefix
                var prefixGet = pm.GetPrefixAsync(Context.Guild?.Id);

                // Prepare message
                eb = EmbedFactory.CreateSuccess()
                    .WithTitle(ts.GetMessage("commands/prefix:embed_title"));

                // Send a message
                reply.AppendLine(MessageFormatter.Format(ts.GetMessage("commands/prefix:my_prefix_is"), PrefixManagerService.PrettyPrefix(await prefixGet)));
                if (Context.IsPrivate)
                    reply.AppendLine(ts.GetMessage("commands/prefix:error_private_messages"));
                eb.Description = reply.ToString();
                await ReplyAsync(embed: eb.Build());

                // Return!
                return;
            }

            // Check if in guild
            if (Context.Guild is null)
            {
                // No
                eb = EmbedFactory.CreateError()
                    .WithTitle(ts.GetMessage("errors:permission_denied"));
                reply.AppendLine(ts.GetMessage("commands/prefix:error_private_messages"));
            }
            else
            {
                // In guild
                string currentPrefix = await pm.GetPrefixAsync(Context.Guild.Id);
                if (currentPrefix == prefix)
                {
                    eb = EmbedFactory.CreateError()
                        .WithTitle(ts.GetMessage("errors:permission_denied"));
                    reply.AppendLine(ts.GetMessage("commands/prefix:error_same_prefix"));
                }
                else
                {
                    // Update the prefix cache
                    await pm.CachePrefix(Context.Guild.Id, prefix);

                    // Send the info message here for seemingly better performance. The prefix is cached in Redis so it will still work.
                    eb = EmbedFactory.CreateSuccess()
                        .WithTitle(ts.GetMessage("commands/prefix:embed_title"));
                    reply.AppendLine(MessageFormatter.Format(ts.GetMessage("commands/prefix:prefix_set_message"),
                        PrefixManagerService.PrettyPrefix(prefix)));

                    // Send message
                    eb.Description = reply.ToString();
                    ReplyAsync(embed: eb.Build()).Release();

                    try
                    {
                        // Set prefix in the actual database.
                        await pm.SetPrefixAsync(Context.Guild.Id, prefix);
                    }
                    catch (Exception e)
                    {
                        // Log the error so that it can be debugged
                        var log = Logger.GetLogger(this);
                        log.Warning($"Failed to store prefix for guild {Context.Guild.Name} ({Context.Guild.Id}): pfx: {prefix} Exception:", e);
                    }
                    return;
                }
            }

            // Send message
            eb.Description = reply.ToString();
            await ReplyAsync(embed: eb.Build());
        }
    }
}