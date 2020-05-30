using System;

namespace DirtBot.Attributes
{
    /// <summary>
    /// Makes the command or module required to have on a guild and makes unable to be removed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class RequiredAttribute : Attribute
    {
        public RequiredAttribute() { }
    }
}
