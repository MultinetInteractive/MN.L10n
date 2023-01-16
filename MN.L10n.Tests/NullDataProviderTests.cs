using MN.L10n.NullProviders;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MN.L10n.Tests
{
    public class NullDataProviderTests
    {
        [Fact]
        public void LoadL10n()
        {
            var provider = CreateNullProvider();
            var l10n = provider.LoadL10n();
            Assert.Empty(l10n.Phrases);
            Assert.Single(l10n.Languages);
        }

        [Fact]
        public async Task ReloadFromSources()
        {
            var provider = CreateNullProvider();
            var l10n = provider.LoadL10n();
            var reloaded = await provider.LoadTranslationFromSources(l10n, true, CancellationToken.None);
            Assert.False(reloaded);
        }

        private NullDataProvider CreateNullProvider()
        {
            return new NullDataProvider();
        }
    }
}
