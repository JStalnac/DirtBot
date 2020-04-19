using DirtBot.Helpers;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace DirtBot.Commands
{
    public class PurgeCommand : ModuleBase<SocketCommandContext>
    {
        [Command("purge")]
        [Alias("purge", "delet")]
        [RequireBotPermission(ChannelPermission.ManageMessages, ErrorMessage = "En voi poistaa viestejä tällä kanavalla")]
        [RequireUserPermission(ChannelPermission.ManageMessages, ErrorMessage = "Et sää sais poistaa näitä edes ite!")]
        public async Task Purge(int limit, [Remainder]string args = null)
        {
            Context.Message.DeleteAsync();
            var messages = await Context.Channel.GetMessagesAsync(limit: limit).FlattenAsync();
            (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);
        }

        [Command("delet this")]
        [Alias("delet this")]
        [RequireBotPermission(ChannelPermission.ManageMessages, ErrorMessage = "Hehehe poistaisin tuon viestin mieluusti, mutta minulla ei ole oikeutta siihen hihih")]
        public async Task Purge([Remainder]string args = null)
        {
            Context.Message.DeleteAsync();
            IUserMessage m = await ReplyAsync("Ole hyvä :)");
            m.DeletAfterDelay(5000);
        }
    }
}
