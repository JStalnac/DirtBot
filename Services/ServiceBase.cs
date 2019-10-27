using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DirtBot.Caching;

namespace DirtBot.Services
{
    /// <summary>
    /// The base service class.
    /// </summary>
    public class ServiceBase
    {
        protected CommandService commands;
        protected DiscordSocketClient discord;
        protected Config config;
        protected IServiceProvider services;
        protected Cache cache;
        protected Emojis emojis;

        public enum SendResult
        {
            Success,
            Invalid,
            NotAllowed,
            Unknown
        }

        /// <summary>
        /// Call this in the constructor to initialize the service.
        /// </summary>
        /// <param name="services"></param>
        protected void InitializeService(IServiceProvider services)
        {
            commands = services.GetRequiredService<CommandService>();
            discord = services.GetRequiredService<DiscordSocketClient>();
            config = services.GetRequiredService<Config>();
            cache = services.GetRequiredService<Cache>();
            emojis = services.GetRequiredService<Emojis>();
            this.services = services;

            // Adding the service.
            commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        #region String utils
        /// <summary>
        /// Chooses a random string from an array or a list.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public string ChooseRandomString(string[] array)
        {
            return array[new Random().Next(0, array.Length)];
        }

        /// <summary>
        /// Chooses a random string from an array or a list.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public string ChooseRandomString(List<string> list)
        {
            return list[new Random().Next(0, list.Count)];
        }

        public string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return char.ToUpper(value[0]) + value.Substring(1);
        }
        #endregion

        #region Send utils
        /// <summary>
        /// Returns true if the bot has permissions to send messages in the provided channel and sends the message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task<SendResult> SendMessageIfAllowed(string message, ISocketMessageChannel channel)
        {
            // Check if the message is null. You cannot send empty messages.
            if (String.IsNullOrWhiteSpace(message)) return SendResult.Invalid;

            try
            {
                await channel.SendMessageAsync(message);
                return SendResult.Success;
            }
            catch (Discord.Net.HttpException e)
            {
                // Some Discord or permission error happened

                // Cannot send messages, doesn't matter
                if (e.DiscordCode == 50013)
                {
                    return SendResult.NotAllowed;
                }
                else // Something else that we don't handle
                {
                    Console.WriteLine($"Error while sending message: DiscordCode: {e.DiscordCode} HttpCode: {e.HttpCode} Exception:\n{e}");
                    return SendResult.Unknown;
                }
            }
        }

        public async Task<SendResult> SendMessageIfAllowed(string message, IMessageChannel channel)
        {
            // Check if the message is null. You cannot send empty messages.
            if (string.IsNullOrWhiteSpace(message)) return SendResult.Invalid;

            try
            {
                await channel.SendMessageAsync(message);
                return SendResult.Success;
            }
            catch (Discord.Net.HttpException e)
            {
                // Some Discord or permission error happened

                // Cannot send messages, doesn't matter
                if (e.DiscordCode == 50013)
                {
                    return SendResult.NotAllowed;
                }
                else // Something else that we don't handle
                {
                    Console.WriteLine($"Error while sending message: DiscordCode: {e.DiscordCode} HttpCode: {e.HttpCode} Exception:\n{e}");
                    return SendResult.Unknown;
                }
            }
        }

        public async Task<SendResult> AddReactionIfAllowed(Emote emote, SocketUserMessage message)
        {
            try
            {
                await message.AddReactionAsync(emote);
                return SendResult.Success;
            }
            catch (Discord.Net.HttpException e)
            {
                if (e.HttpCode == HttpStatusCode.BadRequest)
                {
                    return SendResult.Invalid;
                }
                else if (e.HttpCode == HttpStatusCode.Forbidden)
                {
                    return SendResult.NotAllowed;
                }
                else
                {
                    Console.WriteLine($"Error while adding reaction: DiscordCode: {e.DiscordCode} HttpCode: {e.HttpCode} Exception:\n{e}");
                    return SendResult.Unknown;
                }
            }
        }
        #endregion


        #region Message origin utils
        /// <summary>
        /// Returns true if the channel is a DM Channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool IsDMChannel(ISocketMessageChannel channel)
        {
            SocketGuildChannel guildChannel = channel as SocketGuildChannel;
            if (guildChannel is null) return true; // Conversion test
            try
            {
                // If this works the guild of the channel has a id.
                ulong _ = guildChannel.Guild.Id;
            }
            catch (Exception)
            {
                // DM Channel. No guild id.
                return true;
            }
            // Guild channel. Has a guild id.
            return false;
        }

        /// <summary>
        /// Returns true if the message is a system message. Returns a out parametre userMessage
        /// </summary>
        /// <param name="message"></param>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public bool IsSystemMessage(SocketMessage message, out SocketUserMessage userMessage)
        {
            SocketUserMessage msg = message as SocketUserMessage;
            if (msg is null)
            {
                userMessage = null;
                return true;
            }
            else
            {
                userMessage = msg;
                return false;
            }
        }

        /// <summary>
        /// Filters system and bot messages. Returns true if the message is a bot or system message. Returns a out parametre userMessage
        /// </summary>
        /// <param name="message"></param>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public bool IsBotMessage(SocketMessage message, out SocketUserMessage userMessage)
        {
            SocketUserMessage msg = message as SocketUserMessage;
            if (msg is null)
            {
                userMessage = null;
                return true;
            }
            else if (msg.Author.IsBot)
            {
                userMessage = msg;
                return true;
            }
            else
            {
                userMessage = msg;
                return false;
            }
        }
        #endregion
    }
}
