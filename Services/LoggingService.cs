using DirtBot.Logging;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading;
using System.Threading.Tasks;

namespace DirtBot.Services
{
    public class LoggingService : InitializedService
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;

        public LoggingService(DiscordSocketClient client, CommandService commands)
        {
            this.client = client;
            this.commands = commands;
        }

        public override Task InitializeAsync(CancellationToken cancellationToken)
        {
            client.Log += Log;
            commands.Log += Log;
            return Task.CompletedTask;
        }

        private Task Log(LogMessage msg)
        {
            // A cache could be maybe made but Discord log messages are still rare
            var log = Logger.GetLogger(msg.Source);

            // Write message on correct log level
            log.Write(msg.Message, msg.Severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Info,
                LogSeverity.Verbose => LogLevel.Info,
                LogSeverity.Debug => LogLevel.Debug,
                _ => LogLevel.Info
            }, msg.Exception);
            return Task.CompletedTask;
        }
    }
}
