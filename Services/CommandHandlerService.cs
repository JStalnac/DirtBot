using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DirtBot.Extensions;
using Color = System.Drawing.Color;

namespace DirtBot.Services
{
    public class CommandHandlerService : ServiceBase
    {
        private static bool initialized;
        private PrefixManagerService _prefixManager;

        public CommandHandlerService(PrefixManagerService prefixManager)
        {
            _prefixManager = prefixManager;
        }

        /// <summary>
        /// Initializes the command handler and adds all the commands from the current assembly
        /// </summary>
        /// <param name="pfx">Default prefix to use. Also used in private and group messages.</param>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            if (initialized)
                return;
            initialized = true;
            Commands.CommandExecuted += CommandExecutedAsync;
            Client.MessageReceived += async msg =>
            {
                // Free the Gateway thread from the command task.
                MessageLogger(msg);
                await ProcessCommandAsync(msg);
            };

            await Commands.AddModulesAsync(Assembly.GetExecutingAssembly(), Services);
        }

        private async Task ProcessCommandAsync(SocketMessage arg)
        {
            // Source filter
            if (arg.Source != MessageSource.User) return;
            var message = (SocketUserMessage) arg;

            // Get the prefix
            string pfx = await GetPrefix(arg);

            // Command check
            int argPos = 0;
            if (message.HasStringPrefix(pfx, ref argPos))
            {
                // Refresh the cached prefix
                if (message.Channel is IGuildChannel c)
                    _prefixManager.RestoreCache(c.GuildId).Release();
                var context = new SocketCommandContext(Client, message);
                await Commands.ExecuteAsync(context, argPos, Services);
            }
        }

        public Task<string> GetPrefix(ICommandContext ctx)
        {
            return GetPrefix(ctx.Message);
        }

        public Task<string> GetPrefix(IMessage message)
        {
            if (!initialized)
                return null;
            return _prefixManager.GetPrefixAsync((message.Channel as IGuildChannel)?.GuildId);
        }

        public Task<string> GetPrefix(ulong guildId)
        {
            if (!initialized)
                return null;
            return _prefixManager.GetPrefixAsync(guildId);
        }

        private static void MessageLogger(SocketMessage arg)
        {
            var log = Logger.GetLogger("Messages");
            string content = arg.Content == "" & arg.Embeds.Any() ? "<embed>" : arg.Content;
            log.Write($"Message from {arg.Author}: {content}", Color.DarkGray);
        }

        private Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext ctx, IResult result)
        {
            if (result.IsSuccess)
            {
                var logger = Logger.GetLogger("Commands");
                var sb = new StringBuilder();

                if (!command.IsSpecified)
                    sb.Append("Unspecified command: ");
                if (result.IsSuccess)
                    sb.Append("Executed command: ");

                sb.Append(" src:");
                sb.Append(ctx.Guild != null ? $"{ctx.Guild.Name} ({ctx.Guild.Id})" : "DM");
                sb.Append(" chnl:");
                sb.Append($"{ctx.Channel.Name} ({ctx.Channel.Id})");
                sb.Append(" usr:");
                sb.Append($"{ctx.User.Username}#{ctx.User.Discriminator} ({ctx.User.Id})");
                sb.Append(" cmd:");
                sb.AppendLine(ctx.Message.Content);

                logger.Info(sb.ToString());
            }
            return Task.CompletedTask;
        }
    }
}