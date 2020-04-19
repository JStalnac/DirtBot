using DirtBot.Caching;
using DirtBot.Logging;
using DirtBot.Helpers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using DirtBot.Database;
using System.Runtime.Serialization;

namespace DirtBot.Services
{
    public class CommandHandlingService : ServiceBase
    {
        public static readonly InstanceCache<ulong, string> prefixCache = new InstanceCache<ulong, string>(UpdateType.LastAccess, "Prefixes")
        {
            removeAfterSeconds = 300d
        };

        static ReadOnlyDataCollection<string, string> prefixData = new DataCollectionBuilder<string, string>()
            .Add("prefix", Config.Prefix)
            .BuildReadOnlyDataCollection();

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
                ulong id = (message.Channel as SocketGuildChannel).Guild.Id;
                return GetPrefix(id);
            }
            return Config.Prefix;
        }
        public static string GetPrefix(ulong guildId)
        {
            try
            {
                return prefixCache.Get(guildId);
            }
            catch (KeyNotFoundException)
            {
                // No cached prefix
                var guild = DirtBot.Client.GetGuild(guildId);
                var file = guild.GetStorageFile("prefix.bin");
                ReadOnlyDataCollection<string, string> data;

                try
                {
                    data = file.ReadAsBinary<ReadOnlyDataCollection<string, string>>();
                }
                catch (IOException ex)
                {
                    Logger.Log($"Failed to read prefix from {file.FileName}", true, ex, fore: ConsoleColor.Yellow);
                    data = prefixData;
                }
                catch (SerializationException ex)
                {
                    Logger.Log($"Failed to read prefix from {file.FileName}", true, ex, fore: ConsoleColor.Yellow);
                    // The file is empty or something
                    file.WriteAsBinary(prefixData);
                    data = prefixData;
                }

                // TODO: Catch this
                string prefix = data["prefix"];
                if (String.IsNullOrEmpty(prefix))
                {
                    prefix = Config.Prefix;
                }
                Logger.WriteLogFile($"Loaded prefix for guild {guildId}. Prefix: '{prefix}'");

                // Cache the prefix
                prefixCache.Add(guildId, prefix);
                return prefix;
            }
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
