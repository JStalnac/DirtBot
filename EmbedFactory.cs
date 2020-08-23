using Discord;

namespace DirtBot.Services
{
    /// <summary>
    /// Provides static methods for creating embeds.
    /// </summary>
    public static class EmbedFactory
    {
        public static Color Error { get; } = new Color(0xE63C3C);
        public static Color Success { get; } = new Color(0x11BB21);
        public static Color Neutral { get; } = new Color(0x2E94B6);

        public static EmbedBuilder CreateError() => new EmbedBuilder().WithColor(Error);

        public static EmbedBuilder CreateSuccess() => new EmbedBuilder().WithColor(Success);

        public static EmbedBuilder CreateNeutral() => new EmbedBuilder().WithColor(Neutral);
    }
}
