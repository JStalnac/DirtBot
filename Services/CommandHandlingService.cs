using DirtBot.Caching;
using DirtBot.Logging;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace DirtBot.Services
{
    public class CommandHandlingService : ServiceBase
    {
        public static InstanceCache<ulong, string> prefixCache = new InstanceCache<ulong, string>((pair, now) =>
        {
            if (pair.GetTimeToRemove(now) < 10d)
            {
                return InstanceCache.UpdateResult.Remove;
            }
            else
            {
                return InstanceCache.UpdateResult.Keep;
            }
        });

        public CommandHandlingService(IServiceProvider services)
        {
            InitializeService(services);
            Commands.CommandExecuted += CommandExecutedAsync;
            Client.MessageReceived += MessageReceivedAsync;

            Commands.AddModulesAsync(Assembly.GetExecutingAssembly(), services).GetAwaiter().GetResult();

            string log = "Active modules:\n";
            foreach (var module in Commands.Modules)
            {
                log += $"{module.Name}\n";
            }
            Logger.Log(log, true, foregroundColor: ConsoleColor.Cyan);
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

        public static string GetPrefix(ulong guild)
        {
            try
            {
                return prefixCache.Get(guild);
            }
            catch (KeyNotFoundException)
            {
                // No cached prefix
                prefixCache.Add(guild, Config.Prefix, 1);
                return Config.Prefix;
                // TODO: Get the actual prefix
            }
        }

        async Task MessageReceivedAsync(SocketMessage arg)
        {
            // Just a quick log...
            Logger.Log($"Message from {arg.Author}: {arg.Content}", foregroundColor: ConsoleColor.DarkGray);

            // Source filter
            if (arg.Source != MessageSource.User) return;
            SocketUserMessage message = arg as SocketUserMessage;
            if (message is null) return;

            // eg. dirt prefix
            if (message.Content.ToLower().Trim() == $"{Config.Prefix.ToLower()}prefix")
            {
                arg.Channel.SendMessageAsync($"Prefixini on **'{GetPrefix(arg)}'**");
                // Leave! The PrefixCommand does this too!
                return;
            }

            var argPos = 0;
            if (message.HasStringPrefix(GetPrefix(arg), ref argPos))
            {
                var context = new SocketCommandContext(Client, message);
                await Commands.ExecuteAsync(context, argPos, Services);
            }
        }

        async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;

            if (result.IsSuccess)
                return;

            await context.Channel.SendMessageAsync($"Tapahtui virhe: {result}");
        }
    }
}
