using DirtBot.Logging;
using DirtBot.Services.Options;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DirtBot.Services
{
    public class CustomStatusService : InitializedService
    {
        private readonly DiscordSocketClient client;
        private readonly CustomStatusServiceOptions options;
        private readonly System.Timers.Timer timer;
        private static readonly Random random = new Random();
        private int current = -1;

        public CustomStatusService(DiscordSocketClient client, IOptions<CustomStatusServiceOptions> options)
        {
            this.client = client;
            this.options = options.Value ?? throw new ArgumentNullException(nameof(options));
            timer = new System.Timers.Timer()
            {
                AutoReset = true,
                Interval = TimeSpan.FromMinutes(15).TotalMilliseconds,
                Enabled = false
            };
            timer.Elapsed += Timer_Elapsed;
        }

        public override Task InitializeAsync(CancellationToken cancellationToken)
        {
            client.Connected += () =>
            {
                // Set the initial status
                Timer_Elapsed(this, null);
                timer.Start();
                return Task.CompletedTask;
            };
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Stop();
            return base.StopAsync(cancellationToken);
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!options.Statuses.Any())
                return;
            // No need to update status
            if (current != -1 && options.Statuses.Length == 1)
                return;

            try
            {
                int i = 0;
                int status;
                do
                {
                    i++;
                    status = random.Next(0, options.Statuses.Length);
                } while (status == current || i == 50);
                current = status;
                client.SetGameAsync(options.Statuses[current]).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.GetLogger(this).Warning("Failed to update custom status", ex);
            }
        }
    }
}