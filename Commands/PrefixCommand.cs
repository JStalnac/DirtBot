using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.Commands;
using Discord.WebSocket;
using DirtBot.DataBase;
using DirtBot.DataBase.FileManagement;
using DirtBot.DataBase.DataBaseObjects;

namespace DirtBot.Commands
{
    public class PrefixCommand : ModuleBase<SocketCommandContext>
    {
        object fileLock = new object();

        [Command("prefix")]
        public Task Prefix(string prefix)
        {
            if (!(Context.Channel is SocketGuildChannel))
            {
                ReplyAsync("Et voi vaihtaa prefixiä yksityisviesteissä!");
            }
            else
            {
                // Guild channel
                if (prefix.Length > 12)
                {
                    ReplyAsync("Prefix voi olla maksimissa vain 12 merkkiä pitkä!");
                    return Task.CompletedTask;
                }

                DataBasifier.SetPrefix((Context.Channel as SocketGuildChannel).Guild.Id, prefix);
            }

            return Task.CompletedTask;
        }
    }
}
