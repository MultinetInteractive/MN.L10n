namespace MN.L10n.Javascript.Core
{
    public interface IL10nJavascriptMiddlewareConfig
    {
        bool IncludeTranslations { get; set; }
    }

    public class L10nJavascriptMiddlewareConfig : IL10nJavascriptMiddlewareConfig
    {
        public bool IncludeTranslations { get; set; }
    }
}
