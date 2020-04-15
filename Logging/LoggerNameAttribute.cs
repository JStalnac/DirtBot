using System;

namespace DirtBot.Logging
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class LoggerNameAttribute : Attribute
    {
        public string Name { get; }

        public LoggerNameAttribute(string name)
        {
            Name = name;
        }
    }
}
