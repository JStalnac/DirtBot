using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace DirtBot.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class CooldownPreconditionAttribute : PreconditionAttribute
    {
        public double Cooldown { get; }

        /// <inheritdoc/>
        public override string ErrorMessage { get; set; }

        /// <summary>
        /// Adds cooldown to the guild for the command after the command is executed.
        /// </summary>
        /// <param name="cooldown">Cooldown in seconds</param>
        public CooldownPreconditionAttribute(double cooldown)
        {
            if (cooldown < 0d)
                throw new ArgumentException("Cooldown cannot be less than 0 seconds");

            Cooldown = cooldown;
        }

        /// <inheritdoc/>
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return new Func<Task<PreconditionResult>>(async () =>
            {
                var u = context.User as IGuildUser;
                if (u is null)
                    // Why error when this only checks for guild cooldown
                    return PreconditionResult.FromSuccess();

                var redis = services.GetRequiredService(typeof(ConnectionMultiplexer)) as ConnectionMultiplexer;
                IDatabaseAsync db = redis.GetDatabase(0);

                //db.KeyExistsAsync($"{command.Module.}")

                return PreconditionResult.FromSuccess();
            }).Invoke();
        }
    }
}
