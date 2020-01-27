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
            if (IsSystemMessage(arg, out SocketUserMessage message)) return;

            // Just a quick log...
            DashCMD.WriteLine($"Message from {message.Author}: {message.Content}", ConsoleColor.DarkGray);

            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            if (!message.HasStringPrefix(Config.Prefix, ref argPos)) return;

            var context = new SocketCommandContext(Client, message);
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
