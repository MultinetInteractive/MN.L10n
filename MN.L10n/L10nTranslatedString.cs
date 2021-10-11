using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("MN.L10n.Tests")]
namespace MN.L10n
{
    [JsonConverter(typeof(L10nTranslatedStringJsonConverter))]
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
    
    public class L10nTranslatedStringJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(L10nTranslatedString) || objectType == typeof(L10nTranslatedString?);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.ReadAsString();
            if(value == null) {
                return null;
            }
		
            return new L10nTranslatedString(value);
        }
    }

    public static class L10nTranslatedStringExtensions
    {
        public static IEnumerable<string> ToStringCollection(this IEnumerable<L10nTranslatedString> translatedStrings)
        {
            return translatedStrings?.Select(str => str.ToString());
        }

        public static string[] ToStringArray(this IEnumerable<L10nTranslatedString> translatedStrings)
        {
            return translatedStrings.ToStringCollection()?.ToArray();
        }

        public static List<string> ToStringList(this IEnumerable<L10nTranslatedString> translatedStrings)
        {
            return translatedStrings.ToStringCollection()?.ToList();
        }
    }
}
