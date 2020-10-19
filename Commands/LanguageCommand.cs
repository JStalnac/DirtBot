using DirtBot.Attributes;
using DirtBot.Translation;
using DirtBot.Utilities;
using Discord;
using Discord.Commands;
using System.Globalization;
using System.Threading.Tasks;

namespace DirtBot.Commands
{
    /// <summary>
    /// Commands for changing language used.
    /// </summary>
    public class LanguageCommand : ModuleBase<SocketCommandContext>
    {
        [Command("set-language")]
        [Tags("language", "settings")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "Perms", ErrorMessage = "errors/user:permission_administrator")]
        [RequireUserPermission(GuildPermission.ManageGuild, Group = "Perms", ErrorMessage = "errors/user:permission_manage_guild")]
        public async Task SetLanguage(string language)
        {
            var eb = new EmbedBuilder();
            var ts = await TranslationManager.CreateFor(Context.Channel);
            eb.Title = ts.GetMessage("commands/language:embed_title");

            try
            {
                CultureInfo lang = new CultureInfo(language);
                if (!TranslationManager.HasLanguage(lang))
                {
                    // Please translate :)
                    eb = EmbedFactory.CreateError()
                        .WithTitle(ts.GetMessage("commands/language:embed_title"))
                        .WithDescription(MessageFormatter.Format(ts.GetMessage("commands/language:error_language_not_found"), lang.TwoLetterISOLanguageName));
                }
                else
                {
                    eb.Color = EmbedFactory.Success;
                    eb.Description = MessageFormatter.Format(ts.GetMessage("commands/language:language_set_message"), lang.TwoLetterISOLanguageName);
                    await TranslationManager.SetLanguageAsync(TranslationManager.GetId(Context.Channel), lang);
                }
            }
            catch (CultureNotFoundException)
            {
                eb.Description = ts.GetMessage("commands/language:error_unknown_language");
                eb.Color = EmbedFactory.Error;
            }
            await ReplyAsync(embed: eb.Build());
        }
    }
}
