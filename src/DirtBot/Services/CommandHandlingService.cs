using DirtBot.Logging;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DirtBot.Services
{
    public class CommandHandlingService : ServiceBase
    {
        public CommandHandlingService(IServiceProvider services)
        {
            InitializeService(services);
            Commands.CommandExecuted += CommandExecutedAsync;
            Client.MessageReceived += MessageReceivedAsync;

            Commands.AddModulesAsync(Assembly.GetExecutingAssembly(), services);

            foreach (var m in Commands.Modules)
            {
                
            }
        }

        public static string GetPrefix(SocketMessage message)
        {
            if (message.Channel is SocketGuildChannel)
            {
                ulong id = (message.Channel as SocketGuildChannel).Guild.Id;
                return GetPrefix(id);
            }
            return Config.Prefix;
        }
        public static string GetPrefix(ulong guildId)
        {
            return Config.Prefix;
        }

        async Task MessageReceivedAsync(SocketMessage arg)
        {
            // Just a quick log...
            MessageLogger(arg);

            // Source filter
            if (arg.Source != MessageSource.User) return;
            SocketUserMessage message = arg as SocketUserMessage;

            // eg. dirt prefix
            if (message.Content.ToLower().Trim() == $"{Config.Prefix.ToLower()}prefix")
            {
                arg.Channel.SendMessageAsync($"Prefixini on **'{GetPrefix(arg)}'**");
                // Leave! The PrefixCommand does this too!
                return;
            }

            // Command check
            var argPos = 0;
            if (message.HasStringPrefix(GetPrefix(arg), ref argPos))
            {
                var context = new SocketCommandContext(Client, message as SocketUserMessage);
                await Commands.ExecuteAsync(context, argPos, Services);
            }
        }

        [LoggerName("CommandHandlingService Message Handler")]
        Task MessageLogger(SocketMessage arg)
        {
            Logger.Log($"Message from {arg.Author}: {arg.Content}", fore: ConsoleColor.DarkGray);
            return Task.CompletedTask;
        }

        async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;
            if (result.IsSuccess)
                return;

            Logger.Log($"Command failed: {result}", true, fore: ConsoleColor.Yellow);
            await context.Channel.SendMessageAsync($"Tapahtui virhe: {result}");
        }
    }
}
