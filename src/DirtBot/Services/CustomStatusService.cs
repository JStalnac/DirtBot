using System.Threading.Tasks;
using DirtBot.Extensions;

namespace DirtBot.Services
{
    public class CustomStatusService : ServiceBase
    {
        public CustomStatusService()
        {
            Client.Ready += () =>
            {
                // Set the status of the bot
                Client.SetGameAsync("Being a good dirt blob").Release();
                return Task.CompletedTask;
            };
        }
    }
}