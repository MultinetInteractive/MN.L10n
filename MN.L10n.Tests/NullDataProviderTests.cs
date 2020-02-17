using Microsoft.VisualStudio.TestTools.UnitTesting;
using MN.L10n.NullProviders;
using System.Threading;
using System.Threading.Tasks;

namespace MN.L10n.Tests
{
    [TestClass]
    public class NullDataProviderTests
    {
        [TestMethod]
        public void LoadL10n()
        {
            var provider = CreateNullProvider();
            var l10n = provider.LoadL10n();
            Assert.AreEqual(0, l10n.Phrases.Count);
            Assert.AreEqual(1, l10n.Languages.Count);
        }

        [TestMethod]
        public async Task ReloadFromSources()
        {
            var provider = CreateNullProvider();
            var l10n = provider.LoadL10n();
            var reloaded = await provider.LoadTranslationFromSources(l10n, CancellationToken.None);
            Assert.IsFalse(reloaded);
        }

        private NullDataProvider CreateNullProvider()
        {
            return new NullDataProvider();
        }
    }
}
