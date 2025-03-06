#nullable enable
using System;

namespace MN.L10n.PhraseMetadata
{
    public class PhraseMetadataReplacer
    {
        private const string MetaDataStartToken = " !ctx=";
        
        public static string ReplaceMetadata(string? phrase)
        {
            if (phrase == null)
            {
                return string.Empty;
            }

            if (phrase.Length < MetaDataStartToken.Length)
            {
                return phrase;
            }
            
            var metaDataStartIndex = phrase.IndexOf(MetaDataStartToken, StringComparison.OrdinalIgnoreCase);
            
            if (metaDataStartIndex == -1)
            {
                return phrase;
            }
            
            return phrase.Substring(0, metaDataStartIndex);
        }
    }
}
