using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MN.L10n.NullProviders
{
    public class NullDataProvider : IL10nDataProvider
    {
        public L10n LoadL10n()
        {
            return new L10n
            {
                Languages = new List<L10nLanguageItem> { new L10nLanguageItem { LanguageId = "sv-SE" } },
                Phrases = new ConcurrentDictionary<string, L10nPhrase>()
            };
        }

        public Task<bool> LoadTranslationFromSources(L10n l10n, CancellationToken token)
        {
            return Task.FromResult(false);
        }

        public bool SaveL10n(L10n l10n)
        {
            return true;
        }
    }
}
