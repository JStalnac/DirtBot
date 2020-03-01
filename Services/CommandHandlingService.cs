using System;
using System.Reflection;
using System.Threading.Tasks;
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

        string GetPrefix(SocketMessage message) 
        {
            if (message.Channel is SocketGuildChannel) 
            {
                return Cache[message]["Prefix"];
            }
            else
            {
                return Config.Prefix;
            }
        }

        async Task MessageReceivedAsync(SocketMessage arg)
        {
            // Just a quick log...
            Logger.Log($"Message from {arg.Author}: {arg.Content}", foregroundColor: ConsoleColor.DarkGray);
            
            if (arg.Source != MessageSource.User) return;
            SocketUserMessage message = arg as SocketUserMessage;

            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            if (!message.HasStringPrefix(GetPrefix(arg), ref argPos)) return;

            var context = new SocketCommandContext(Client, message as SocketUserMessage);
            await Commands.ExecuteAsync(context, argPos, Services);
        }

        async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            Logger.Log("Executed command!", foregroundColor: ConsoleColor.DarkGray);

            if (!command.IsSpecified)
                return;
            if (result.IsSuccess)
                return;

            Logger.Log($"Command failed: {result}", true, foregroundColor: ConsoleColor.Yellow);

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
