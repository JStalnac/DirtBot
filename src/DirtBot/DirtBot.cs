using DSharpPlus;
using System;
using System.Threading.Tasks;

namespace DirtBot
{
    public class DirtBot
    {
        public static DiscordClient Client { get; private set; }
        Logger logger = new Logger("DirtBot");

        public async Task StartAsync()
        {
            #region Client configuration and initialization
            // Here's the bot configuration
            var config = new DiscordConfiguration()
            {
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug,
            };

            // Safely setting the token.
            try
            {
                config.Token = Config.Token;
            }
            catch (ArgumentNullException) { /* Oh no the token is invalid oh faaa >:) */ }

            Client = new DiscordClient(config);
            #endregion
            
            Client.Ready += async (e) =>
            {
                logger.Info("Ready");
            };

            Client.MessageCreated += async (e) =>
            {
                string username = $"{e.Author.Username}#{e.Author.Discriminator}";
                string guild = e.Guild is null ? "DM" : e.Guild.Name;
                logger.Debug($"Message from: {username}@{guild}:{e.Message.Content}");
            };

            Client.ClientErrored += async (e) =>
            {
                new Logger("Exception Logger").Error(null, e.Exception);
            };
            Client.SocketErrored += async (e) =>
            {
                new Logger("Exception Logger").Error(null, e.Exception);
            };

            // File logging
            Client.DebugLogger.LogMessageReceived += Logger.DebugLogger_LogMessageReceived;
            
            // Executed when Ctrl+C is pressed. "Get off there!" Makes the bot go offline instantly when it is off
            Console.CancelKeyPress += async (sender, e) =>
            {
                logger.Info("Disconnecting");
                await Client.DisconnectAsync();
            };

            // Connecting to Discord
            try
            {
                logger.Info("Connecting to Discord");
                await Client.ConnectAsync();
            }
            catch (Exception e)
            {
                // This is why we don't want to throw in the prefix setting part.
                logger.Error("Failed to connect to Discord.", e);
            }

            await Task.Delay(-1);
        }
    }
}
