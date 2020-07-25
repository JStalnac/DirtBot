using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace DirtBot.Commands
{
    /// <summary>
    /// Used for debugging error messages
    /// </summary>
    [RequireOwner]
    public class Errorer : ModuleBase<SocketCommandContext>
    {
        [Command("error")]
        [Alias("err")]
        public async Task ErrorCommand()
        {
            throw new Exception("Error!");
        }
    }
}