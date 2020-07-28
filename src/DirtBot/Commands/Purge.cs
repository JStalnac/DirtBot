﻿using DirtBot.Helpers;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using DirtBot.Extensions;

namespace DirtBot.Commands
{
    public class Purge : ModuleBase<SocketCommandContext>
    {
        // TODO: Modernize and translate
        [Command("purge")]
        [Alias("purge", "delet")]
        [RequireBotPermission(ChannelPermission.ManageMessages,
            ErrorMessage = "En voi poistaa viestejä tällä kanavalla")]
        [RequireUserPermission(ChannelPermission.ManageMessages, ErrorMessage = "Et sää sais poistaa näitä edes ite!")]
        public async Task PurgeCommand(int limit, [Remainder] string args = null)
        {
            await Context.Message.DeleteAsync();
            var messages = await Context.Channel.GetMessagesAsync(limit).FlattenAsync();
            (Context.Channel as ITextChannel)?.DeleteMessagesAsync(messages).Release();
        }

        [Command("delet this")]
        [Alias("delet this")]
        [RequireBotPermission(ChannelPermission.ManageMessages,
            ErrorMessage = "Hehehe poistaisin tuon viestin mieluusti, mutta minulla ei ole oikeutta siihen hihih")]
        public async Task PurgeCommand([Remainder] string args = null)
        {
            await Context.Message.DeleteAsync();
            var m = await ReplyAsync("Ole hyvä :)");
            m.DeleteAfterDelay(5000).Release();
        }
    }
}