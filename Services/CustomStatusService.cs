using System.Threading;
using System.Threading.Tasks;
using DirtBot.Utilities;
using Discord.Addons.Hosting;
using Discord.WebSocket;

namespace DirtBot.Services
{
    public class CustomStatusService : InitializedService
    {
        private readonly DiscordSocketClient client;

        public CustomStatusService(DiscordSocketClient client)
        {
            this.client = client;
        }

        public override Task InitializeAsync(CancellationToken cancellationToken)
        {
            client.Ready += () =>
            {
                // Set the status of the bot
                // TODO: Make settings
                client.SetGameAsync("Being a good dirt blob").Release();
                return Task.CompletedTask;
            };
            return Task.CompletedTask;
        }
    }
}