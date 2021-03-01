using System.Diagnostics;

namespace MN.L10n
{
    [DebuggerDisplay("TranslatedString: {_translatedString}")]
    public readonly struct L10nTranslatedString
    {
        private readonly string _translatedString;

        internal L10nTranslatedString(string translatedString)
        {
            _translatedString = translatedString;
        }

        public static implicit operator string(L10nTranslatedString str) => str._translatedString;

        public override string ToString()
        {
            return _translatedString;
        }
    }
}
