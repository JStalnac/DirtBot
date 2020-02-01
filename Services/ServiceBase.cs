using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Dash.CMD;
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
        protected CommandService Commands;
        protected DiscordSocketClient Client;
        protected static IServiceProvider Services;
        protected Emojis Emojis;
        private Cache cache;

        protected Cache Cache { get => cache;  }

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
            Commands = services.GetRequiredService<CommandService>();
            Client = services.GetRequiredService<DiscordSocketClient>();
            cache = services.GetRequiredService<Cache>();
            Emojis = services.GetRequiredService<Emojis>();
            Services = services;

            // Adding the service.
            Commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        #region String utils
        /// <summary>
        /// Chooses a random string from an array or a list.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        protected string ChooseRandomString(string[] array)
        {
            return array[new Random().Next(0, array.Length)];
        }

        /// <summary>
        /// Chooses a random string from an array or a list.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        protected string ChooseRandomString(List<string> list)
        {
            return list[new Random().Next(0, list.Count)];
        }

        protected string Capitalize(string value)
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
        protected async Task<SendResult> SendMessageIfAllowed(string message, ISocketMessageChannel channel)
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
                    DashCMD.WriteError($"Error while sending message: DiscordCode: {e.DiscordCode} HttpCode: {e.HttpCode} Exception:\n{e}");
                    return SendResult.Unknown;
                }
            }
        }

        protected async Task<SendResult> SendMessageIfAllowed(string message, IMessageChannel channel)
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
                    DashCMD.WriteError($"Error while sending message: DiscordCode: {e.DiscordCode} HttpCode: {e.HttpCode} Exception:\n{e}");
                    return SendResult.Unknown;
                }
            }
        }

        protected async Task<SendResult> AddReactionIfAllowed(Emoji emoji, SocketUserMessage message) 
        {
            return await AddReactionIfAllowed(Emote.Parse(emoji.ToString()), message);
        }

        protected async Task<SendResult> AddReactionIfAllowed(Emote emote, SocketUserMessage message)
        {
            if (emote is null) return SendResult.Invalid;

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
                    DashCMD.WriteError($"Error while adding reaction: DiscordCode: {e.DiscordCode} HttpCode: {e.HttpCode} Exception:\n{e}");
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
        protected bool IsDMChannel(ISocketMessageChannel channel)
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
        protected bool IsSystemMessage(SocketMessage message, out SocketUserMessage userMessage)
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
        protected bool IsBotMessage(SocketMessage message, out SocketUserMessage userMessage)
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
