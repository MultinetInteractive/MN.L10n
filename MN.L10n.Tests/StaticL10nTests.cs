using FakeItEasy;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace MN.L10n.Tests
{
    public class StaticL10nTests
    {
        // We cannot guarantee that L10n have not been initialized before running this test.
        //[TestMethod]
        //public async Task ReloadFromSourceNotInitialized()
        //{
        //    try
        //    {
        //        await L10n.ReloadFromDataProviderSources(CancellationToken.None);
        //        Assert.Fail("Not initialized exception expected");
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message == null || !ex.Message.Contains("L10n.CreateInstance"))
        //        {
        //            throw;
        //        }
        //    }
        //}

        [Fact]
        public async Task ReloadFromSources()
        {
            CreateFakes();
            var reloaded = await L10n.ReloadFromDataProviderSources(true, CancellationToken.None);
            Assert.False(reloaded);
        }

        [Fact]
        public async Task ReloadFromSources2()
        {
            var fakes = CreateFakes();
            A.CallTo(() => fakes.DataProvider.LoadTranslationFromSources(fakes.L10n, true, CancellationToken.None))
                .Returns(Task.FromResult(true));
            var reloaded = await L10n.ReloadFromDataProviderSources(true, CancellationToken.None);
            Assert.True(reloaded);
        }

        [Fact]
        public async Task ReloadFromSourcesDoesNotCallHookWhenReloadFails()
        {
            CreateFakes();
            L10n.TranslationsReloaded += (sender, args) => throw new XunitException("Unexpected call to reloaded hook");
            await L10n.ReloadFromDataProviderSources(true, CancellationToken.None);
        }

        [Fact]
        public async Task ReloadFromSourceCallsHookWhenReloadSucceeds()
        {
            var fakes = CreateFakes();
            A.CallTo(() => fakes.DataProvider.LoadTranslationFromSources(fakes.L10n, true, CancellationToken.None))
                .Returns(Task.FromResult(true));
            var hookWasCalled = false;
            L10n.TranslationsReloaded += (sender, args) => hookWasCalled = true;
            await L10n.ReloadFromDataProviderSources(true, CancellationToken.None);
            Assert.True(hookWasCalled, "Translations reloaded was not called");
        }

        [Fact]
        public async Task RemoveTranslationReloadedListeners()
        {
            var fakes = CreateFakes();
            L10n.TranslationsReloaded += (sender, args) => throw new XunitException("Unexpected call to reloaded hook");
            A.CallTo(() => fakes.DataProvider.LoadTranslationFromSources(fakes.L10n, true, CancellationToken.None))
                .Returns(Task.FromResult(true));

            L10n.RemoveAllTranslationReloadedListeners();
            await L10n.ReloadFromDataProviderSources(true, CancellationToken.None);
        }

        private class Fakes
        {
            public L10n L10n { get; set; }
            public IL10nLanguageProvider LanguageProvider { get; }
            public IL10nDataProvider DataProvider { get; }

            public Fakes()
            {
                LanguageProvider = A.Fake<IL10nLanguageProvider>();
                DataProvider = A.Fake<IL10nDataProvider>();
            }
        }

        private Fakes CreateFakes()
        {
            var fakes = new Fakes();
            var l10n = L10n.CreateInstance(fakes.LanguageProvider, fakes.DataProvider);
            fakes.L10n = l10n;
            return fakes;
        }

        [Fact]
        public void CleanupTests()
        {
            L10n.RemoveAllTranslationReloadedListeners();
        }
    }
}
