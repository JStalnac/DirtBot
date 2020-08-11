using Discord.Commands;
using DirtBot.Translation;
using System.Threading.Tasks;
using Discord;
using System.Globalization;
using SmartFormat;

namespace DirtBot.Commands
{
    /// <summary>
    /// Commands for changing language used.
    /// </summary>
    public class LanguageCommand : ModuleBase<SocketCommandContext>
    {
        [Command("set-language")]
        public async Task SetLanguage(string language)
        {
            // TODO: Check permissions
            var eb = new EmbedBuilder();
            var ts = await TranslationManager.CreateFor(Context.Channel);
            eb.Title = ts.GetMessage("commands/language:embed_title");

            CultureInfo lang;
            try
            {
                lang = new CultureInfo(language);
                eb.Color = new Color(0x00ff00);
                eb.Description = ts.GetMessage("commands/language:language_set_message").FormatSmart(lang.TwoLetterISOLanguageName);

                Logger.GetLogger(this).Info(lang.TwoLetterISOLanguageName);
                await TranslationManager.SetLanguageAsync(TranslationManager.GetId(Context.Channel), lang);
            }
            catch (CultureNotFoundException)
            {
                eb.Description = ts.GetMessage("commands/language:error_unknown_language");
                eb.Color = new Color(0xff0000);
            }
            await ReplyAsync(embed: eb.Build());
        }
    }
}
