using System;
using System.Linq;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using DirtBot.Database;
using DirtBot.Database.Models;
using DirtBot.Services;
using Discord;
using Microsoft.EntityFrameworkCore;

namespace DirtBot.Commands
{
    public class Prefix : ModuleBase<SocketCommandContext>
    {
        [Command("set_prefix")]
        [Alias("prefix")]
        public async Task PrefixCommand(string prefix, [Remainder] string args = null)
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
                using (var db = new DatabaseContext())
                {
                    // Update prefix
                    var gPrefix = new GuildPrefix()
                    {
                        Id = Context.Guild.Id,
                        Prefix = prefix
                    };
                    if (db.Prefixes.Any(p => p.Id == gPrefix.Id))
                    {
                        // Prefix already exists in database
                        db.Prefixes.Attach(gPrefix);
                        db.Entry(gPrefix).State = EntityState.Modified;
                    }
                    else
                    {
                        // No prefix yet stored
                        db.Prefixes.Add(gPrefix);
                    }

                    try
                    {
                        // Save the database changes
                        db.SaveChanges();

                        eb.Title = "Prefix";
                        eb.Color = new Color(0x00ff00);
                        reply.AppendLine($"This server's prefix is now **{prefix}**");
                    }
                    catch (Exception e)
                    {
                        // Error
                        eb.Title = "Oops!";
                        eb.Color = new Color(0xff0000);
                        reply.AppendLine("Failed to store prefix");
                        reply.AppendLine("Try again later");
                        // Log the error so that it can be debugged
                        var log = Logger.GetLogger(this);
                        log.Warning($"Failed to store prefix for guild {Context.Guild.Name} ({Context.Guild.Id}): pfx: {prefix} Exception:", e);
                    }
                }

                // Hint about spaces
                if (!(args is null))
                {
                    reply.AppendLine();
                    reply.AppendLine("Hint: You can add \" around the text to have spaces in your prefix.```\"Like this\"```");
                }
            }

            // Send message
            eb.Description = reply.ToString();
            await ReplyAsync(embed: eb.Build()).ConfigureAwait(false);
        }

        // [Command("get_prefix")]
        // [Alias("prefix")]
        // public async Task Prefix()
        // {
        //     // Get prefix
        //     string prefix = CommandHandlerService.GetPrefix(Context);
        //     if (!Context.IsPrivate)
        //         prefix = CommandHandlerService.GetPrefix(Context.Guild.Id);
        //
        //     // Prepare message
        //     var eb = new EmbedBuilder();
        //     var reply = new StringBuilder();
        //     eb.Title = "Prefix";
        //     eb.Color = new Color(0x00ff00);
        //
        //     // Send a message
        //     reply.AppendLine($"My prefix is **{prefix}**");
        //     if (Context.IsPrivate)
        //         reply.AppendLine("We are in private messages, so you can't change the prefix here.");
        //     eb.Description = reply.ToString();
        //     await ReplyAsync(embed: eb.Build()).ConfigureAwait(false);
        // }
    }
}