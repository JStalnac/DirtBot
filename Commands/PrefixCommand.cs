using System;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using DirtBot.Services;
using Discord;
using Microsoft.EntityFrameworkCore;

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
                try
                {
                    // Set prefix
                    await pm.SetPrefixAsync(Context.Guild.Id, prefix);

                    eb.Title = "Prefix";
                    eb.Color = new Color(0x00ff00);
                    string p = prefix.Contains(' ') ? $"'{prefix}'" : prefix;
                    reply.AppendLine($"The server's prefix is now **{p}**");
                }
                catch (DbUpdateConcurrencyException)
                {
                    eb.Title = "Oops!";
                    eb.Color = new Color(0xff0000);
                    reply.AppendLine("An error occured while storing your prefix.");
                    reply.AppendLine("The prefix was recently updated and caused an error");
                    reply.AppendLine("Please try again later.");

                    var log = Logger.GetLogger(this);
                    log.Warning($"Concurrency error while storing prefix for guild {Context.Guild.Name} ({Context.Guild.Id}): pfx: {prefix}");
                }
                catch (Exception e)
                {
                    // Error
                    eb.Title = "Oops!";
                    eb.Color = new Color(0xff0000);
                    reply.AppendLine("An error occured while storing your prefix.");
                    reply.AppendLine("Please try again later.");
                    // Log the error so that it can be debugged
                    var log = Logger.GetLogger(this);
                    log.Warning($"Failed to store prefix for guild {Context.Guild.Name} ({Context.Guild.Id}): pfx: {prefix} Exception:", e);
                }

                // Hint about spaces
                if (!(args is null))
                {
                    reply.AppendLine();
                    reply.AppendLine("Hint: You can add **\"** around the text to have spaces in your prefix.```\"Like this\"```");
                }
            }

            // Send message
            eb.Description = reply.ToString();
            await ReplyAsync(embed: eb.Build()).ConfigureAwait(false);
        }

        [Command("get_prefix")]
        [Alias("prefix")]
        public async Task Prefix()
        {
            // Get prefix
            string prefix = await pm.GetPrefixAsync(Context.Guild?.Id);

            // Prepare message
            var eb = new EmbedBuilder();
            var reply = new StringBuilder();
            eb.Title = "Prefix";
            eb.Color = new Color(0x00ff00);

            // Send a message
            reply.AppendLine($"My prefix is **{prefix}**");
            if (Context.IsPrivate)
                reply.AppendLine("We are in private messages, so you can't change the prefix here.");
            eb.Description = reply.ToString();
            await ReplyAsync(embed: eb.Build()).ConfigureAwait(false);
        }
    }
}