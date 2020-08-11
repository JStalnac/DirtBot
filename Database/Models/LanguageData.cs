using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirtBot.Database.Models
{
    public class TranslationData
    {
        /// <summary>
        /// The user or guild that this translation should be used with.
        /// </summary>
        [Key]
        [Column(TypeName = "BIGINT")]
        public ulong Id { get; set; }

        [Required]
        public string Language
    }
}