namespace DirtBot.Services
{
    public class CustomStatusService : ServiceBase
    {
        public CustomStatusService()
        {
            Client.Ready += async () =>
            {
                // Set the status of the bot
                Client.SetGameAsync("Being a good dirt blob").ConfigureAwait(false);
            };
        }
    }
}