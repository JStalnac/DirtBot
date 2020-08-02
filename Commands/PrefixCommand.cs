using System;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using DirtBot.Extensions;
using DirtBot.Services;
using Discord;

namespace DirtBot.Commands
{
    public class PrefixCommand : ModuleBase<SocketCommandContext>
    {
        private readonly PrefixManagerService pm;

        public PrefixCommand(PrefixManagerService prefixManager)
        {
            pm = prefixManager;
        }

        [Command("set_prefix")]
        [Alias("prefix")]
        public async Task Prefix(string prefix, [Remainder] string args = null)
        {
            // Prepare reply
            var eb = new EmbedBuilder();
            var reply = new StringBuilder();

            // Check if in guild
            if (Context.Guild is null)
            {
                // No
                eb.Title = "You can't do that!";
                eb.Color = new Color(0xff0000);
                reply.AppendLine("You can't change the prefix in private messages");
            }
            else
            {
                // In guild
                string currentPrefix = await pm.GetPrefixAsync(Context.Guild.Id);
                if (currentPrefix == prefix)
                {
                    eb.Title = "You can't do that!";
                    eb.Color = new Color(0xff0000);
                    reply.AppendLine("That is the current prefix of this guild!");
                }
                else
                {
                    // Update the prefix cache
                    await pm.CachePrefix(Context.Guild.Id, prefix);

                    // Send the info message here for seemingly better performance. The prefix is cached in Redis so it will still work.
                    eb.Title = "Prefix";
                    eb.Color = new Color(0x00ff00);
                    reply.AppendLine($"The server's prefix is now **{PrefixManagerService.PrettyPrefix(prefix)}**");

                    // Hint about spaces
                    if (!(args is null))
                    {
                        reply.AppendLine();
                        reply.AppendLine("Hint: You can add **\"** around the text to have spaces in your prefix.```\"Like this\"```");
                    }

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

        [Command("get_prefix")]
        [Alias("prefix")]
        public async Task Prefix()
        {
            // Get prefix
            var prefixGet = pm.GetPrefixAsync(Context.Guild?.Id);

            // Prepare message
            var eb = new EmbedBuilder();
            var reply = new StringBuilder();
            eb.Title = "Prefix";
            eb.Color = new Color(0x00ff00);

            // Send a message
            reply.AppendLine($"My prefix is **{PrefixManagerService.PrettyPrefix(await prefixGet)}**");
            if (Context.IsPrivate)
                reply.AppendLine("We are in private messages, so you can't change the prefix here.");
            eb.Description = reply.ToString();
            await ReplyAsync(embed: eb.Build()).ConfigureAwait(false);
        }
    }
}