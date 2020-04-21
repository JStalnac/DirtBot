using System;
using System.IO;

namespace DirtBot
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                try
                {
                    File.AppendAllText("log.txt", $"The application has thrown an unhandled exception: {e.ExceptionObject}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write to log file. {ex}");
                }
                try
                {
                    // This is the safe-zone to do all the stuff when the bot goes offline
                    if (DirtBot.Client != null)
                        DirtBot.Client.DisconnectAsync();
                }
                catch (Exception) { }
            };

            string PadCenter(string s, int width, char c)
            {
                if (s == null || width <= s.Length) return s;

                int padding = width - s.Length;
                return s.PadLeft(s.Length + padding / 2, c).PadRight(width, c);
            }

            string restart = " -[ RESTART ]- ";
            restart = PadCenter(restart, 90, '=');
            File.AppendAllText("log.txt", restart + "\n");

            new DirtBot().StartAsync().GetAwaiter().GetResult();
        }
    }
}
