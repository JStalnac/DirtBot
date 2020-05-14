using System;

namespace DirtBot.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class RequiredAttribute : Attribute
    {
        public RequiredAttribute() { }
    }
}
