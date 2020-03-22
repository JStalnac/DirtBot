using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DirtBot.Commands
{
    public class PurgeCommand : ModuleBase<SocketCommandContext>
    {
        [Command("purge")]
        [Alias("purge", "delet")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task Purge(int limit, [Remainder]string[] args) 
        {
            var messages = await Context.Channel.GetMessagesAsync(limit: limit).FlattenAsync();
            (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);
        }

        [Command("purge")]
        [Alias("delet this")]
        public async Task Purge([Remainder]string[] args) 
        {
            ReplyAsync("Ole hyvä :)");
            Context.Message.DeleteAsync();
        }
    }
}
