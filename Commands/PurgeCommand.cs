using DirtBot.Attributes;
using DirtBot.Attributes.Preconditions;
using DirtBot.Extensions;
using DirtBot.Translation;
using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

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
            await (Context.Channel as ITextChannel)?.DeleteMessagesAsync(messages);
            // Respond
            var ts = await TranslationManager.CreateFor(Context.Channel);
            await Context.Channel.SendMessageFormatted(ts.GetMessage("commands/purge:delete_success"), args: messages.Count());
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