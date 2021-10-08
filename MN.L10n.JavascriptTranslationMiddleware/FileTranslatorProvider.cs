using System.Collections.Concurrent;

namespace MN.L10n.JavascriptTranslationMiddleware
{
    public interface ITranslatorProvider
    {
        FileTranslator GetOrCreateTranslator(string languageId, IFileHandle fileHandle);
    }
    
    public class FileTranslatorProvider : ITranslatorProvider
    {
        private readonly ConcurrentDictionary<string, FileTranslator> _fileProviders = new();
        private readonly IJavascriptTranslationL10nLanguageProvider _l10NLanguageProvider;

        public FileTranslatorProvider(IJavascriptTranslationL10nLanguageProvider l10NLanguageProvider)
        {
            _l10NLanguageProvider = l10NLanguageProvider;
        }

        public FileTranslator GetOrCreateTranslator(string languageId, IFileHandle fileHandle)
        {
            var fileProviderId = $"{languageId}__{fileHandle.RelativeRequestPath}";
            
            return _fileProviders.GetOrAdd(fileProviderId, _ => new FileTranslator(_l10NLanguageProvider, fileHandle, languageId));
        }
    }
}
