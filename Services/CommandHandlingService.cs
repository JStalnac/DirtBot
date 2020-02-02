using System;
using System.Reflection;
using System.Threading.Tasks;
using Dash.CMD;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DirtBot.DataBase.FileManagement;
using System.Collections.Generic;

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
        }

        async Task MessageReceivedAsync(SocketMessage arg)
        {
            // Just a quick log...
            DashCMD.WriteLine($"Message from {arg.Author}: {arg.Content}", ConsoleColor.DarkGray);

            if (arg.Source != MessageSource.User) return;
            SocketUserMessage message = arg as SocketUserMessage;

            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            if (!message.HasStringPrefix(Config.Prefix, ref argPos)) return;

            var context = new SocketCommandContext(Client, message as SocketUserMessage);
            await Commands.ExecuteAsync(context, argPos, Services);
        }

        async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;

            if (result.IsSuccess)
                return;

            DashCMD.WriteWarning($"Command failed: {result}");

            if (result.Error == CommandError.BadArgCount) 
            {
                await SendMessageIfAllowed("Hupsista! Liian vähän argumentteja!", context.Channel);
            }
            else
            {
                await SendMessageIfAllowed($"Tapahtui virhe: {result}", context.Channel);
            }
        }
    }
}
