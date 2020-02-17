using System.Threading;
using System.Threading.Tasks;

namespace MN.L10n
{
    public interface IL10nDataProvider
    {
        L10n LoadL10n();
        bool SaveL10n(L10n l10n);
        bool SaveTranslation(L10n l10n);

        Task<bool> LoadTranslationFromSources(L10n l10n, CancellationToken token);
    }
}
