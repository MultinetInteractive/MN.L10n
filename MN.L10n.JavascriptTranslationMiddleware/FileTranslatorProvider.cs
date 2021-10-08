using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<FileTranslator> _logger;

        public FileTranslatorProvider(IJavascriptTranslationL10nLanguageProvider l10NLanguageProvider, ILogger<FileTranslator> logger)
        {
            _l10NLanguageProvider = l10NLanguageProvider;
            _logger = logger;
        }

        public FileTranslator GetOrCreateTranslator(string languageId, IFileHandle fileHandle)
        {
            var fileProviderId = $"{languageId}__{fileHandle.RelativeRequestPath}";
            
            return _fileProviders.GetOrAdd(fileProviderId, _ => new FileTranslator(_l10NLanguageProvider, fileHandle, languageId, _logger));
        }
    }
}
