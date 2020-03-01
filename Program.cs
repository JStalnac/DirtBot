namespace DirtBot

{
    class Program
    {
        static void Main(string[] args) 
        {
            Logger.Log("Starting! Hello World!");
            new DirtBot().StartAsync().GetAwaiter().GetResult();
        }
    }
}
