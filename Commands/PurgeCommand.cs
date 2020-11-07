using DirtBot.Attributes;
using DirtBot.Attributes.Preconditions;
using DirtBot.Utilities;
using DirtBot.Translation;
using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DirtBot.Commands
{
    public class PurgeCommand : ModuleBase<SocketCommandContext>
    {
        [Command("purge")]
        [Alias("purge", "delet", "delete")]
        [Tags("moderation")]
        [RequireBotPermission(ChannelPermission.ManageMessages, ErrorMessage = "errors/bot:permission_manage_messages", NotAGuildErrorMessage = "errors:not_a_guild")]
        [RequireUserPermission(ChannelPermission.ManageMessages, ErrorMessage = "errors/user:permission_manage_messages", NotAGuildErrorMessage = "errors:not_a_guild")]
        public async Task Purge(int limit, [Remainder] string args = null)
        {
            // Delete the command
            await Context.Message.DeleteAsync();
            // Get messages and delete
            var messages = await Context.Channel.GetMessagesAsync(limit).FlattenAsync();
            bool failedToDelete = false;
            try
            {
                await (Context.Channel as ITextChannel)?.DeleteMessagesAsync(messages);
            }
            catch (ArgumentOutOfRangeException)
            {
                // One or more of messages older than two weeks
                failedToDelete = true;
            }
            // Respond
            var ts = await TranslationManager.CreateFor(Context.Channel);
            string message;
            if (failedToDelete)
                message = ts.GetMessage("commands/purge:error_older_than_two_weeks");
            else
                message = ts.GetMessage("commands/purge:delete_success");
            // Send response
            await Context.Channel.SendMessageFormatted(message, args: messages.Count())
                .ContinueWith(async msg => await msg.Result.DeleteAfterDelay(5000)).ConfigureAwait(false);
        }

        [Command("delet this")]
        [Alias("delet this")]
        [Categories("fun")]
        [Tags("fun")]
        [RequireBotPermission(ChannelPermission.ManageMessages, ErrorMessage = "errors/bot:permission_manage_messages", NotAGuildErrorMessage = "errors:not_a_guild")]
        public async Task Purge([Remainder] string args = null)
        {
            // Delete message
            await Context.Message.DeleteAsync();
            // Respond
            var ts = await TranslationManager.CreateFor(Context.Channel);
            var m = await ReplyAsync(ts.GetMessage("commands/purge:delet_this_success"));
            m.DeleteAfterDelay(5000).Release();
        }
    }
}