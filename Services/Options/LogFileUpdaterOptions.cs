using System;

namespace DirtBot.Services.Options
{
    public class LogFileUpdaterOptions
    {
        public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromMinutes(15);
    }
}
