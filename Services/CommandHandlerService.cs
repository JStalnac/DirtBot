using DirtBot.Extensions;
using DirtBot.Logging;
using DirtBot.Translation;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace DirtBot.Services
{
    public class CommandHandlerService : InitializedService
    {
        private readonly IServiceProvider services;
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly PrefixManagerService pm;

        public CommandHandlerService(IServiceProvider services, DiscordSocketClient client, CommandService commands, PrefixManagerService pm)
        {
            this.services = services;
            this.client = client;
            this.commands = commands;
            this.pm = pm;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            commands.CommandExecuted += CommandExecutedAsync;
            client.MessageReceived += msg =>
            {
                // Free the Gateway thread from the command task.
                MessageLogger(msg);
                ProcessCommandAsync(msg).Release();
                return Task.CompletedTask;
            };
            await commands.AddModulesAsync(Assembly.GetExecutingAssembly(), services);
        }

        private async Task ProcessCommandAsync(SocketMessage arg)
        {
            // Source filter
            if (arg.Source != MessageSource.User) return;
            var message = (SocketUserMessage)arg;

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
                var context = new SocketCommandContext(client, message);
                // Log message for debugging (doesn't check if the command exists)
                Logger.GetLogger("Commands").Debug($"Executing command: {GetExecutionInfo(context)}");
                // Execute command
                await commands.ExecuteAsync(context, argPos, services);
            }
            else if (message.Content.TrimEnd() == $"{pm.DefaultPrefix}prefix")
            {
                // Info
                var context = new SocketCommandContext(client, message);
                Logger.GetLogger("Commands").Debug($"Executing prefix get with default prefix: {GetExecutionInfo(context)}");
                var ts = await TranslationManager.CreateFor(context.Channel);

                message.Channel.SendMessageAsync(embed: EmbedFactory.CreateSuccess()
                    .WithTitle(ts.GetMessage("commands/prefix:embed_title"))
                    .WithDescription(MessageFormatter.Format(ts.GetMessage("commands/prefix:my_prefix_is"),
                        PrefixManagerService.PrettyPrefix(pfx)))
                    .Build()).Release();
            }
        }

        public Task<string> GetPrefix(ICommandContext ctx) => GetPrefix(ctx.Message);
        
        public Task<string> GetPrefix(IMessage message) => pm.GetPrefixAsync((message.Channel as IGuildChannel)?.GuildId);

        public Task<string> GetPrefix(ulong guildId) => pm.GetPrefixAsync(guildId);

        private static void MessageLogger(SocketMessage arg)
        {
            var log = Logger.GetLogger("Messages");
            string content = arg.Content == "" & arg.Embeds.Any() ? "<embed>" : arg.Content;
            log.Write($"Message from {arg.Author}: {content}", Color.DarkGray);
        }

        private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext ctx, IResult result)
        {
            // Some error occured in command execution
            if (!result.IsSuccess)
            {
                var ts = await TranslationManager.CreateFor(ctx.Channel);
                string message;
                if (command.IsSpecified)
                {
                    try
                    {
                        message = ts.GetMessage(result.ErrorReason);
                    }
                    catch (FormatException)
                    {
                        message = ts.GetMessage("errors:command_errored");
                    }
                }
                else
                    message = ts.GetMessage("command_not_found");
                
                await ctx.Channel.SendMessageAsync(message)
                    .ContinueWith(t => 
                    {
                        if (!t.IsCompletedSuccessfully)
                            Logger.GetLogger("Commands").Info($"Failed to send error message: {t.Exception.InnerException.Message}");
                        else
                            t.Result.DeleteAfterDelay(5000).GetAwaiter().GetResult();
                    }).ConfigureAwait(false);
            }

            // Log
            var sb = new StringBuilder();
            if (!command.IsSpecified)
                sb.Append("Unspecified command: ");
            else
                sb.Append("Executed command: ");
            sb.AppendLine(GetExecutionInfo(ctx));
            Logger.GetLogger("Commands").Info(sb.ToString());
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