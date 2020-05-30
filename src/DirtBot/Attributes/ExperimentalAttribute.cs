using System;

namespace DirtBot.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class ExperimentalAttribute : Attribute
    { }
}
