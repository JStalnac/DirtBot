using System;
using System.Linq;

namespace DirtBot.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TagsAttribute : Attribute
    {
        public string[] Tags { get; }

        public TagsAttribute(params string[] tags)
        {
            if (tags.Length == 0)
                throw new ArgumentNullException(nameof(tags));
            if (tags.Any(x => String.IsNullOrEmpty(x?.Trim())))
                throw new ArgumentException("Tags must not be null or empty", nameof(tags));
            Tags = tags;
        }
    }
}
