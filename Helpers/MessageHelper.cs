using Discord;
using System.Threading.Tasks;

namespace DirtBot.Helpers
{
    public static class MessageHelper
    {
        public static async Task DeletAfterDelay(this IMessage m, int milliseconds)
        {
            await Task.Delay(milliseconds);
            m.DeleteAsync();
        }
    }
}
