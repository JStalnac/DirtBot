using System.Threading.Tasks;

namespace DirtBot.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>n
        /// Releases the task to execute asynchronously hiding the CS4014 warning message without disturbing actual warnings.
        /// </summary>
        /// <param name="task"></param>
        public static void Release(this Task task) { }
    }
}