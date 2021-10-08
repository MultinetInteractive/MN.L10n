using System.Collections.Concurrent;

namespace MN.L10n.JavascriptTranslationMiddleware
{
    public interface ITranslatorProvider
    {
        FileTranslator GetOrCreateTranslator(string languageId);
    }
    
    public class TranslatorProvider : ITranslatorProvider
    {
        private readonly ConcurrentDictionary<string, FileTranslator> _fileProviders = new();
        private readonly IJavascriptTranslationL10nLanguageProvider _l10NLanguageProvider;

        public TranslatorProvider(IJavascriptTranslationL10nLanguageProvider l10NLanguageProvider)
        {
            _l10NLanguageProvider = l10NLanguageProvider;
        }

        public FileTranslator GetOrCreateTranslator(string languageId)
        {
            return _fileProviders.GetOrAdd(languageId, _ => new FileTranslator(_l10NLanguageProvider, languageId));
        }
    }
}
