using DirtBot.Logging;
using DirtBot.Services.Options;
using Discord.Addons.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DirtBot.Services
{
    public class LogFileUpdaterService : InitializedService
    {
        private readonly System.Timers.Timer timer;
        private readonly LogFileUpdaterOptions options;

        private const string NameFormat = "yyyyMMddHHmmss";

        public LogFileUpdaterService(IOptions<LogFileUpdaterOptions> options)
        {
            if (options.Value is null)
                throw new ArgumentNullException(nameof(options));
            this.options = options.Value;
            timer = new System.Timers.Timer
            {
                AutoReset = true,
                Interval = Math.Min(TimeSpan.FromMinutes(1).TotalMilliseconds, this.options.UpdateInterval.TotalMilliseconds),
                Enabled = false
            };
            timer.Elapsed += Timer_Elapsed;
        }

        public override Task InitializeAsync(CancellationToken cancellationToken)
        {
            timer.Start();
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Stop();
            timer.Dispose();
            return base.StopAsync(cancellationToken);
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Logger.LogFile = GetLogFile(DateTime.Now);
        }

        public static string GetLogFile(DateTime now)
        {
            string filename = $"logs/{now.ToString(NameFormat)}{{0}}.log";
            string f = String.Format(filename, "");
            if (File.Exists(f))
            {
                for (int i = 1; i <= 10; i++)
                {
                    f = String.Format(filename, $"_{i}");
                    if (!File.Exists(f))
                        break;
                }
            }
            return f;
        }
    }
}
