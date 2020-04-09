using System;

namespace DirtBot.Logging
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    sealed class LoggerNameAttribute : Attribute
    {
        public LoggerNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
