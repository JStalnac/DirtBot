using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

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

        public static string GetPrefix(SocketMessage message)
        {
            if (message.Channel is SocketGuildChannel) 
            {
                return new Caching.Cache()[message]["Prefix"];
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
            
            // Source filter
            if (arg.Source != MessageSource.User) return;
            SocketUserMessage message = arg as SocketUserMessage;

            if (message.Source != MessageSource.User) return;

            // eg. dirt prefix
            if (message.Content.ToLower().Trim() == $"{Config.Prefix.ToLower()}prefix")
            {
                arg.Channel.SendMessageAsync($"Prefixini on '{GetPrefix(arg)}'");
                // Leave! The PrefixCommand does this too!
                return;
            }

            // Command check
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
