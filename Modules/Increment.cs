using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using DirtBot.DataBase;

namespace DirtBot.Modules
{
    public class Increment : ModuleBase<SocketCommandContext>
    {
        [Command("incr")]
        public Task IncrementValue()
        {
            ServiceProvider services = DirtBot.Services;

            GuildLookup guilds = services.GetRequiredService<GuildLookup>();

            GuildDataLookUp lookup = guilds[Context.Guild.Id];
            IncrementTable incrementLookup = (IncrementTable)lookup["increment"];
            long increment = incrementLookup.Get("increment");
            
            guilds[Context.Guild.Id]["increment"].Set("increment", increment + 1);

            ReplyAsync((increment + 1).ToString());
            return Task.CompletedTask;
        }
    }
}
