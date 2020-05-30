using System;

namespace DirtBot.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class UserCooldownAttribute : Attribute
    {
        public double Cooldown { get; }

        /// <summary>
        /// Adds cooldown for the user after the command is executed.
        /// </summary>
        /// <param name="cooldown">Cooldown in seconds</param>
        public UserCooldownAttribute(double cooldown)
        {
            if (cooldown < 0d)
                throw new ArgumentException("Cooldown cannot be less than 0 seconds");

            Cooldown = cooldown;
        }
    }
}
