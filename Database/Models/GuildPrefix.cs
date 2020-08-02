using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirtBot.Database.Models
{
    public class GuildPrefix
    {
        /// <summary>
        /// The id of the guild that this prefix is used with.
        /// </summary>
        [Key]
        [Column(TypeName = "BIGINT")]
        public ulong Id { get; set; }

        /// <summary>
        /// The prefix of the guild.
        /// </summary>
        [Column(TypeName = "VARCHAR(30)")]
        [Required]
        public string Prefix { get; set; }
    }
}