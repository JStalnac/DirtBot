
namespace DirtBot
{
    class Program
    {
        static void Main(string[] args) => new DirtBot().StartAsync().GetAwaiter().GetResult();
    }
}
