using DirtBot.Core;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Tests_and_Examples
{
    public class ModerationModule : CommandModule
    {
        // Internal name
        public override string Name => "moderation";

        public override string DisplayName => "Moderation";

        // DirtBot services
        public ModerationModule(IServiceProvider services) : base(services) { }

        // Commands
        #region Kick
        [Command("kick")]
        [Aliases("yoink")]
        [RequireGuild]
        [RequireBotPermissions(Permissions.KickMembers | Permissions.Administrator)]
        [RequireUserPermissions(Permissions.BanMembers | Permissions.KickMembers | Permissions.Administrator | Permissions.ManageGuild)]
        public async Task KickCommand(CommandContext ctx, [Description("The user to kick.")] DiscordMember user, [RemainingText, Description("Reason this user was kicked.")] string reason = "No reason given :c")
        {
            // Save this event
            var db = GetStorage(ctx.Guild) as IDatabaseAsync;

            // Get a case ID
            long caseId = await db.StringIncrementAsync("cases");

            // Kick the member
            try
            {
                await user.RemoveAsync($"Kicked by {ctx.User.Username}#{ctx.User.Discriminator} for reason: {reason}");
            }
            catch (BadRequestException)
            {
                var eb = new DiscordEmbedBuilder()
                    .WithAuthor($"{ctx.User.Username}#{ctx.User.Discriminator}", iconUrl: ctx.User.AvatarUrl)
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Reason is too long!")
                    .Build();
                await ctx.RespondAsync(embed: eb);
                return;
            }

            // Log
            await db.HashSetAsync($"kicks:{user.Id}:{caseId}", new ModEvent(caseId, user.Id, ctx.User.Id, DateTime.UtcNow, reason).ToHashEntry());

            var embed = new DiscordEmbedBuilder()
                .WithAuthor($"{ctx.User.Username}#{ctx.User.Discriminator}", iconUrl: ctx.User.AvatarUrl)
                .WithDescription($"Kicked user **{user.Username}#{user.Discriminator}**")
                .AddField("Reason", reason)
                .WithColor(Colors.Green)
                .Build();
            await ctx.RespondAsync(embed: embed);
        }
        #endregion

        #region Ban
        [Command("ban")]
        [Aliases("yeet")]
        [RequireGuild]
        [RequireBotPermissions(Permissions.BanMembers | Permissions.Administrator)]
        [RequireUserPermissions(Permissions.BanMembers | Permissions.Administrator | Permissions.ManageGuild)]
        public async Task BanCommand(CommandContext ctx, [Description("The user to ban.")] DiscordMember user, [RemainingText, Description("Reason this user was banned.")] string reason = "No reason given :c")
        {
            // Save this event
            var db = GetStorage(ctx.Guild) as IDatabaseAsync;

            // Get a case ID
            long caseId = await db.StringIncrementAsync("cases");

            // Ban the member
            try
            {
                await user.BanAsync(reason: $"Banned by {ctx.User.Username}#{ctx.User.Discriminator} for reason: {reason}");
            }
            catch (BadRequestException)
            {
                var eb = new DiscordEmbedBuilder()
                    .WithAuthor($"{ctx.User.Username}#{ctx.User.Discriminator}", iconUrl: ctx.User.AvatarUrl)
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Reason is too long!")
                    .Build();
                await ctx.RespondAsync(embed: eb);
                return;
            }

            // Log
            await db.HashSetAsync($"bans:{user.Id}:{caseId}", new ModEvent(caseId, user.Id, ctx.User.Id, DateTime.UtcNow, reason).ToHashEntry());

            var embed = new DiscordEmbedBuilder()
                .WithAuthor($"{ctx.User.Username}#{ctx.User.Discriminator}", iconUrl: ctx.User.AvatarUrl)
                .WithDescription($"Banned user **{user.Username}#{user.Discriminator}**")
                .AddField("Reason", reason)
                .WithColor(Colors.Green)
                .Build();
            await ctx.RespondAsync(embed: embed);
        }
        #endregion

        #region Warning
        [Command("warn")]
        [RequireGuild]
        [Aliases("warning")]
        [Description("Give a warning to a badly behaving member.")]
        [RequireUserPermissions(Permissions.Administrator | Permissions.KickMembers | Permissions.MuteMembers | Permissions.BanMembers | Permissions.ManageGuild)]
        public async Task WarnMemberCommand(CommandContext ctx, [Description("User to warn.")] DiscordUser user, [RemainingText, Description("Reason this warning was given.")] string reason = "No reason given :c")
        {
            var db = GetStorage(ctx.Guild) as IDatabaseAsync;

            // Get a case ID
            long caseId = await db.StringIncrementAsync("cases");

            await db.HashSetAsync($"warns:{user.Id}:{caseId}", new ModEvent(caseId, user.Id, ctx.User.Id, DateTime.UtcNow, reason).ToHashEntry());

            var embed = new DiscordEmbedBuilder()
                .WithAuthor($"{ctx.User.Username}#{ctx.User.Discriminator}", iconUrl: ctx.User.AvatarUrl)
                .WithColor(Colors.Green)
                .WithDescription($"Warned user **{user.Username}#{user.Discriminator}**")
                .AddField("Reason", reason)
                .Build();
            await ctx.RespondAsync(embed: embed);
        }
        #endregion
    }

    struct ModEvent
    {
        public long Id { get; set; }
        public ulong User { get; set; }
        public ulong Moderator { get; set; }
        public long Timestamp { get; set; }
        public string Reason { get; set; }

        public ModEvent(long id, ulong user, ulong moderator, long timestamp, string reason)
        {
            Id = id;
            User = user;
            Moderator = moderator;
            Timestamp = timestamp;
            Reason = reason;
        }

        public ModEvent(long id, ulong user, ulong moderator, DateTime timestamp, string reason)
        {
            Id = id;
            User = user;
            Moderator = moderator;
            Timestamp = ((DateTimeOffset)timestamp).ToUnixTimeSeconds();
            Reason = reason;
        }

        public HashEntry[] ToHashEntry()
        {
            return new HashEntry[]
            {
                new HashEntry("id", Id),
                new HashEntry("user", User),
                new HashEntry("moderator", Moderator),
                new HashEntry("time", Timestamp),
                new HashEntry("reason", Reason)
            };
        }
    }
}
