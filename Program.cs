using Dash.CMD;

namespace DirtBot

{
    class Program
    {
        static void Main(string[] args) 
        {
            DashCMD.Start();
            DashCMD.AllowCMDExit = false;
            DashCMD.Title = "DirtBot";

            new DirtBot().StartAsync().GetAwaiter().GetResult();
        }
    }
}
