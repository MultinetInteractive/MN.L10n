using System.Collections.Generic;

namespace MN.L10n
{
    public class PhrasesRewriter
    {
        internal L10n _phrases;
        internal Dictionary<string, Dictionary<object, L10nPhraseObject>> _phraseDic = new Dictionary<string, Dictionary<object, L10nPhraseObject>>();

        private readonly List<string> unusedPhrases = new List<string>();
        public PhrasesRewriter(L10n phraseRepo) : base()
        {
            _phrases = phraseRepo;

            var allPhrases = _phrases.Phrases.Keys;
            foreach (var p in allPhrases)
            {
                _phraseDic.Add(p, new Dictionary<object, L10nPhraseObject>());
                unusedPhrases.Add(p);
                _phrases.Phrases[p].Usages = 0;
            }

            foreach (var fLang in _phrases.LanguagePhrases)
            {
                var langKey = fLang.Key;
                var translatedPhrases = fLang.Value.Phrases;
                foreach (var trpr in translatedPhrases)
                {
                    if (!_phraseDic.ContainsKey(trpr.Key))
                    {
                        _phraseDic.Add(trpr.Key, new Dictionary<object, L10nPhraseObject>());
                    }
                    _phraseDic[trpr.Key].Add(langKey, trpr.Value);
                }
            }
        }

        public bool SavePhrasesToFile()
        {
            foreach (var unused in unusedPhrases)
            {
                _phrases.Phrases.TryRemove(unused, out _);
            }

            return L10n.SaveDataProvider();
        }
    }
}
