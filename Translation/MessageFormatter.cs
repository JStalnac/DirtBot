using DirtBot.Logging;
using Discord;
using SmartFormat;
using System;
using System.Threading.Tasks;

namespace DirtBot.Translation
{
    /// <summary>
    /// Provides static methods for formatting messages.
    /// </summary>
    public static class MessageFormatter
    {
        private static Logger Log { get; } = Logger.GetLogger("Formatting");

        /// <summary>
        /// Formats a message with provided arguments. Logs errors in the console.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Format(string format, params object[] args)
        {
            if (String.IsNullOrEmpty(format?.Trim()))
                throw new ArgumentNullException(nameof(format));

            try
            {
                return Smart.Format(format, args);
            }
            catch (Exception e)
            {
                Log.Warning("Failed to format message", e);
                return format;
            }
        }

        /// <summary>
        /// Sends a formatted message to the text channel. Logs errors in the console.
        /// </summary>
        /// <param name="channel">Text channel</param>
        /// <param name="format">Format</param>
        /// <param name="isTTS">Is the message Text To Speech</param>
        /// <param name="embed">Embed</param>
        /// <param name="args">Format arguments</param>
        /// <returns></returns>
        public static Task<IUserMessage> SendMessageFormatted(this IMessageChannel channel, string format, bool isTTS = false, Embed embed = null, params object[] args)
        {
            if (channel is null)
                throw new ArgumentNullException(nameof(channel));
            return channel.SendMessageAsync(Format(format, args), isTTS, embed);
        }
    }
}
