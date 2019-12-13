using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DirtBot;
using System.Collections.Generic;

namespace DirtBot.Services
{
    public class CommandHandlingService : ServiceBase
    {
        public CommandHandlingService(IServiceProvider services)
        {
            InitializeService(services);
            commands.CommandExecuted += CommandExecutedAsync;
            discord.MessageReceived += MessageReceivedAsync;
        }

        async Task MessageReceivedAsync(SocketMessage arg)
        {
            if (IsSystemMessage(arg, out SocketUserMessage message)) return;

            // Just a quick log...
            await logger.VerboseAsync($"Message from {message.Author}: {message.Content}");

            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            if (!message.HasStringPrefix(config.Prefix, ref argPos)) return;

            var context = new SocketCommandContext(discord, message);
            await commands.ExecuteAsync(context, argPos, services);
        }

        async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;

            if (result.IsSuccess)
                return;

            await SendMessageIfAllowed($"error: {result}", context.Channel);
        }
    }
}
