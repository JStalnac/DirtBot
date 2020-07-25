using System.ComponentModel.DataAnnotations;

namespace DirtBot.Database.Models
{
    public class GuildPrefix
    {
        [Key]
        public ulong GuildId { get; set; }

        public string Prefix { get; set; }
    }
}