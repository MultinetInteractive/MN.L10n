using System;
using FakeItEasy;
using System.Collections.Generic;
using Xunit;

namespace MN.L10n.Tests
{
    public class TransactionLanguageScopeTests : IDisposable
    {
        [Fact]
        public void CheckStackIsWorking()
        {
            CreateFakes();
            using (L10n.CreateLanguageScope("1"))
            {
                Assert.Equal("1", L10n.GetLanguage());
                using (L10n.CreateLanguageScope("2"))
                {
                    Assert.Equal("2", L10n.GetLanguage());
                    using (L10n.CreateLanguageScope("3"))
                    {
                        Assert.Equal("3", L10n.GetLanguage());
                    }
                    Assert.Equal("2", L10n.GetLanguage());
                }
                Assert.Equal("1", L10n.GetLanguage());
            }
        }

        private class Fakes
        {
            public L10n L10n { get; set; }
            public IL10nLanguageProvider LanguageProvider { get; }
            public IL10nDataProvider DataProvider { get; }
            public IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();

            public Fakes()
            {
                LanguageProvider = A.Fake<IL10nLanguageProvider>();
                DataProvider = A.Fake<IL10nDataProvider>();
            }
        }

        private Fakes CreateFakes()
        {
            var fakes = new Fakes();
            var l10n = L10n.CreateInstance(fakes.LanguageProvider, fakes.DataProvider, () => fakes.Items);
            fakes.L10n = l10n;
            return fakes;
        }

        public void Dispose()
        {
            L10n.RemoveAllTranslationReloadedListeners();
        }
    }
}
