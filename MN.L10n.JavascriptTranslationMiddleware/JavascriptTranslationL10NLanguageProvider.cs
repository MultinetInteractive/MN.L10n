using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MN.L10n.Tests")]
namespace MN.L10n.JavascriptTranslationMiddleware
{
    public interface IJavascriptTranslationL10nLanguageProvider
    {
        bool TryGetLanguage(string languageId, [MaybeNullWhen(false)] out L10nLanguage l10NLanguage);
    }

    internal class JavascriptTranslationL10NLanguageProvider : IJavascriptTranslationL10nLanguageProvider
    {
        public bool TryGetLanguage(string languageId, [MaybeNullWhen(false)] out L10nLanguage l10NLanguage)
        {
            try
            {
                l10NLanguage = L10n.GetL10nLanguage(languageId);
                return true;
            }
            catch
            {
                l10NLanguage = null;
                return false;
            }
        }
    }
}
