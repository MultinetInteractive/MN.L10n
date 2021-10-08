using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MN.L10n.JavascriptTranslationMiddleware
{
    public class FileTranslator
    {
        private readonly string _languageId;
        private readonly IJavascriptTranslationL10nLanguageProvider _languageProvider;
        private readonly IFileHandle _fileHandle;
        private readonly Lazy<TranslatedFileInformation> _lazyFileInformation; 
        private readonly SemaphoreSlim _translateSemaphor = new(1);
        private readonly ILogger _logger;

        public FileTranslator(IJavascriptTranslationL10nLanguageProvider languageProvider, IFileHandle fileHandle, string languageId, ILogger<FileTranslator> logger)
        {
            _languageId = languageId;
            _logger = logger;
            _fileHandle = fileHandle;
            _languageProvider = languageProvider;
            _lazyFileInformation = new Lazy<TranslatedFileInformation>(GetFileInformation);
            L10n.TranslationsReloaded += L10nOnTranslationsReloaded;
            _logger = logger;
        }

        private void L10nOnTranslationsReloaded(object? sender, EventArgs e)
        {
            HandleTranslationsChangedAsync().GetAwaiter();
        }
        
        public async Task<TranslatedFileInformation> TranslateFile(bool reuseExisting)
        {
            var fileInformation = _lazyFileInformation.Value;
            await _translateSemaphor.WaitAsync();

            try
            {
                if (File.Exists(fileInformation.FilePath) && reuseExisting)
                {
                    _logger.LogTrace("File already exists, reusing it");
                    return fileInformation;
                }

                await TranslateAndWriteTranslatedFileAsync(fileInformation);
                return fileInformation;
            }
            finally
            {
                _translateSemaphor.Release();
            }
        }
        
        public string TranslateFileContents(string contents)
        {
            if (!_languageProvider.TryGetLanguage(_languageId, out var language)) return contents;

            var parser = new L10nParser();
            List<L10nParser.PhraseInvocation>? invocations = parser.Parse(contents, true);
            Dictionary<int, L10nParser.PhraseInvocation>? starts = invocations.ToDictionary(i => i.StartChar);

            var pluralPhrases = new Dictionary<string, L10nPhraseObject>();

            StringBuilder translationBuilder = new();
            for (var i = 0; i < contents.Length; i++)
            {
                if (starts.TryGetValue(i + 1, out var invocation) &&
                    language.Phrases.TryGetValue(invocation.Phrase, out var languagePhrase))
                {
                    if (languagePhrase.r.Count > 1)
                    {
                        // Om översättningen innehåller pluralizering behöver vi ha kvar originalet och översätta runtime
                        pluralPhrases[invocation.Phrase] = languagePhrase;
                    }
                    else
                    {
                        var translation = languagePhrase.r.First().Value;
                        translationBuilder.Append(translation);

                        if (invocation.IsEscaped) translationBuilder.Append('\\');

                        i = invocation.EndChar - 1;

                        continue;
                    }
                }

                translationBuilder.Append(contents[i]);
            }

            var translatedCode = translationBuilder.ToString();
            if (pluralPhrases.Count == 0) return translatedCode;

            StringBuilder addPhrasesCode = new("var x = l10n.Phrases;");
            foreach (var (key, value) in pluralPhrases)
                addPhrasesCode.Append($"x[\"{key}\"] = {JsonConvert.SerializeObject(value)};");

            return $@"(function(){{ {addPhrasesCode} }})();{Environment.NewLine}{translatedCode}";
        }

        private async Task HandleTranslationsChangedAsync()
        {
            _logger.LogTrace("Updating translated file because of translations change");
            await _translateSemaphor.WaitAsync();
            try
            {
                await TranslateAndWriteTranslatedFileAsync(_lazyFileInformation.Value);
            }
            finally
            {
                _translateSemaphor.Release();
            }
        }

        private async Task TranslateAndWriteTranslatedFileAsync(TranslatedFileInformation fileInfo)
        {
            var contents = await _fileHandle.GetFileContentsAsync();
            _logger.LogTrace("Translating file contents");
            var translatedContents = TranslateFileContents(contents);
            await using var fileWriter = File.CreateText(fileInfo.FilePath);
            _logger.LogTrace($"Writing translated contents to disk at {fileInfo.FilePath}");
            await fileWriter.WriteAsync(translatedContents);
        }

        private TranslatedFileInformation GetFileInformation()
        {
            var parts = _fileHandle.Path.Split(Path.DirectorySeparatorChar);
            var tmpNameParts = _fileHandle.FileName.Split(".");
            
            var extension = tmpNameParts[^1];
            var fileName = string.Join(".", tmpNameParts.SkipLast(1));

            var newFileName = fileName + $"__{_languageId}.{extension}";

            var filePath = string.Join(Path.DirectorySeparatorChar, parts.SkipLast(1).Append(newFileName));
            var relativePath = string.Join("/", _fileHandle.RelativeRequestPath.Split("/").SkipLast(1).Append(newFileName));

            return new TranslatedFileInformation(filePath, relativePath);
        }

        ~FileTranslator()
        {
            L10n.TranslationsReloaded -= L10nOnTranslationsReloaded;
        }
    }

    public class TranslatedFileInformation
    {
        public string FilePath { get; }
        public string RelativeRequestPath { get; }
        public TranslatedFileInformation(string filePath, string relativeRequestPath)
        {
            FilePath = filePath;
            RelativeRequestPath = relativeRequestPath;
        }
    }
}
