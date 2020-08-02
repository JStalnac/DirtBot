using System;
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
        private PrefixManagerService pm;

        public CommandHandlerService(PrefixManagerService pm)
        {
            this.pm = pm;
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
            Client.MessageReceived += msg =>
            {
                // Free the Gateway thread from the command task.
                MessageLogger(msg);
                ProcessCommandAsync(msg).Release();
                return Task.CompletedTask;
            };

            await Commands.AddModulesAsync(Assembly.GetExecutingAssembly(), Services);
        }

        private async Task ProcessCommandAsync(SocketMessage arg)
        {
            // Source filter
            if (arg.Source != MessageSource.User) return;
            var message = (SocketUserMessage) arg;

            // Get the prefix
            string pfx;
            try
            {
                pfx = await GetPrefix(arg);
            }
            catch (Exception e)
            {
                string src = message.Channel is IGuildChannel gc ? $"{gc.Guild.Name} ({gc.Guild.Id})" : $"{message.Channel.Name}";
                Logger.GetLogger("Commands").Warning($"Failed to get prefix. {src}", e);
                return;
            }

            // Command check
            int argPos = 0;
            if (message.HasStringPrefix(pfx, ref argPos))
            {
                // Refresh the cached prefix
                if (message.Channel is IGuildChannel c)
                    pm.RestoreCache(c.GuildId).Release();
                var context = new SocketCommandContext(Client, message);
                // Log message for debugging
                Logger.GetLogger("Commands").Debug($"Executing command: {GetExecutionInfo(context)}");
                // Execute command
                await Commands.ExecuteAsync(context, argPos, Services);
            }
            else if (message.Content.TrimEnd() == $"{pm.DefaultPrefix}prefix")
            {
                // Info
                var context = new SocketCommandContext(Client, message);
                Logger.GetLogger("Commands").Debug($"Executing prefix get with default prefix: {GetExecutionInfo(context)}");

                message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle("Title")
                    .WithDescription($"My prefix is {PrefixManagerService.PrettyPrefix(pfx)}")
                    .Build()).Release();
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
            return pm.GetPrefixAsync((message.Channel as IGuildChannel)?.GuildId);
        }

        public Task<string> GetPrefix(ulong guildId)
        {
            if (!initialized)
                return null;
            return pm.GetPrefixAsync(guildId);
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
                sb.AppendLine(GetExecutionInfo(ctx));
                logger.Info(sb.ToString());
            }
            return Task.CompletedTask;
        }

        private string GetExecutionInfo(ICommandContext ctx)
        {
            if (ctx is null)
                throw new ArgumentNullException(nameof(ctx));
            var sb = new StringBuilder();
            sb.Append("src:");
            sb.Append(ctx.Guild != null ? $"{ctx.Guild.Name} ({ctx.Guild.Id}) " : "DM ");
            sb.Append("chnl:");
            sb.Append($"{ctx.Channel.Name} ({ctx.Channel.Id}) ");
            sb.Append("usr:");
            sb.Append($"{ctx.User.Username}#{ctx.User.Discriminator} ({ctx.User.Id}) ");
            sb.Append("cmd:");
            sb.Append(ctx.Message.Content);
            return sb.ToString();
        }
    }
}