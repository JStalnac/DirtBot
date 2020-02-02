using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DirtBot.Services
{
    public class FsInTheChat : ServiceBase
    {
        public FsInTheChat(IServiceProvider services) 
        {
            InitializeService(services);
            Client.MessageReceived += MessageReviecedAsync;
        }

        async Task MessageReviecedAsync(SocketMessage message)
        {
            if (message.Source != Discord.MessageSource.User) return;
            if (message.Author.Id == Client.CurrentUser.Id) return; // Don't respond to ourselves! That will make a bloooody mess!

            if (message.Content.ToLower().Trim() == "f")
            {
                if (IsDMChannel(message.Channel))
                { 
                    await message.Channel.SendMessageAsync("F");
                }
                else
                {
                    long fCount = Cache[message]["fCount"];
                    long maxfCount = Cache[message]["maxfCount"];

                    fCount++;
                    if (fCount >= maxfCount) 
                    {
                        await SendMessageIfAllowed("F", message.Channel);
                        fCount = 0;
                    }
                    Cache[message]["fCount"] = fCount;
                }
            }
        }
    }
}
