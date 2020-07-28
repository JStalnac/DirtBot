using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DirtBot.Database;
using DirtBot.Extensions;
using Color = System.Drawing.Color;

namespace DirtBot.Services
{
    public class CommandHandlerService : ServiceBase
    {
        private static string prefix;
        private static bool initialized;

        /// <summary>
        /// Initializes the command handler and adds all the commands from the current assembly
        /// </summary>
        /// <param name="pfx">Default prefix to use. Also used in private and group messages.</param>
        /// <returns></returns>
        public async Task InitializeAsync(string pfx)
        {
            if (initialized)
                return;
            initialized = true;
            prefix = pfx;
            Commands.CommandExecuted += CommandExecutedAsync;
            Client.MessageReceived += async msg =>
            {
                // Free the Gateway thread from the command task.
                MessageLogger(msg);
                await ProcessCommandAsync(msg);
            };

            await Commands.AddModulesAsync(Assembly.GetExecutingAssembly(), Services);
        }

        private async Task ProcessCommandAsync(SocketMessage arg)
        {
            /*
             * TODO: Currently command execution is very slow because of
             * a large overhead with the database connection and prefix
             * retrieval. I suggest we implement some kind of caching ourselves
             * with StackExchange.Redis or with a different third party caching
             * library like Microsoft.Extensions.Caching.Memory
             */

            // Source filter
            if (arg.Source != MessageSource.User) return;
            var message = (SocketUserMessage) arg;

            // Get the prefix
            string pfx = GetPrefix(arg);

            // Prefix get here so that it won't be inefficiently in another class
            if (message.Content.ToLower().Trim() == $"{pfx}prefix")
            {
                // Prepare message
                var eb = new EmbedBuilder();
                var reply = new StringBuilder();
                eb.Title = "Prefix";
                eb.Color = new Discord.Color(0x00ff00); // TODO: Cool colors with some kind of custom class :)

                // Send a message
                reply.AppendLine($"My prefix is **{pfx}**");
                if (message.Channel is IPrivateChannel)
                    reply.AppendLine("We are in private messages, so you can't change the prefix here.");
                eb.Description = reply.ToString();
                message.Channel.SendMessageAsync(embed: eb.Build()).Release();
                return;
            }

            // Command check
            int argPos = 0;
            if (message.HasStringPrefix(pfx, ref argPos))
            {
                var context = new SocketCommandContext(Client, message);
                await Commands.ExecuteAsync(context, argPos, Services);
            }
        }

        public static string GetPrefix(ICommandContext ctx)
        {
            return GetPrefix(ctx.Message);
        }

        public static string GetPrefix(IMessage message)
        {
            if (!initialized)
                return null;
            if (message.Channel is IGuildChannel channel)
                return GetPrefix(channel.GuildId);
            return prefix;
        }

        public static string GetPrefix(ulong guildId)
        {
            if (!initialized)
                return null;
            using (var db = new DatabaseContext())
                return db.Prefixes.SingleOrDefault(p => p.Id == guildId)?.Prefix ?? prefix;
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
                    sb.Append("Unspecified command");
                if (result.IsSuccess)
                    sb.Append("Executed command");

                sb.Append(" src:");
                sb.Append(ctx.Guild != null ? $"{ctx.Guild.Name} ({ctx.Guild.Id})" : "DM");
                sb.Append(" chnl:");
                sb.Append($"{ctx.Channel.Name} ({ctx.Channel.Id})");
                sb.Append(" usr:");
                sb.Append($"{ctx.User.Username}#{ctx.User.Discriminator} ({ctx.User.Id})");
                sb.Append(" cmd:");
                sb.AppendLine(ctx.Message.Content);

                logger.Info(sb.ToString());
            }
            return Task.CompletedTask;
        }
    }
}