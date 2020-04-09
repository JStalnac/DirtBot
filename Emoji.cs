namespace DirtBot
{
    public class Emoji
    {
        public ulong Id { get; private set; }
        public string Name { get; private set; }
        public bool IsAnimated { get; private set; }

        public Emoji(ulong id, string name, bool isAnimated)
        {
            Id = id;
            Name = name;
            IsAnimated = isAnimated;
        }

        public override string ToString() => $"<{(IsAnimated ? "a" : "")}:{Name}:{Id}>";
    }
}
