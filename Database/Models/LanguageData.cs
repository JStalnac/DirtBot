using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace DirtBot.Database.Models
{
    public class LanguageData
    {
        /// <summary>
        /// The user or guild that this translation should be used with.
        /// </summary>
        [Key]
        public ulong Id { get; set; }

        private string language;

        /// <summary>
        /// The language used.
        /// </summary>
        [Required]
        public string Language
        {
            get => language;
            set
            {
                // Check that the new language is a proper culture
                new CultureInfo(value);
                language = value;
            }
        }
    }
}