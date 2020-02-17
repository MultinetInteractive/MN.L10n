using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MN.L10n.Tests
{
    [TestClass]
    public class TransactionLanguageScopeTests
    {
        [TestMethod]
        public void CheckStackIsWorking()
        {
            CreateFakes();
            using (L10n.CreateLanguageScope("1"))
            {
                Assert.AreEqual("1", L10n.GetLanguage());
                using (L10n.CreateLanguageScope("2"))
                {
                    Assert.AreEqual("2", L10n.GetLanguage());
                    using (L10n.CreateLanguageScope("3"))
                    {
                        Assert.AreEqual("3", L10n.GetLanguage());
                    }
                    Assert.AreEqual("2", L10n.GetLanguage());
                }
                Assert.AreEqual("1", L10n.GetLanguage());
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

        [TestCleanup]
        public void CleanupTests()
        {
            L10n.RemoveAllTranslationReloadedListeners();
        }
    }
}
