using System;
using Microsoft.AspNetCore.Http;

namespace MN.L10n.JavascriptTranslationMiddleware;

public class VersionedFileRedirectConfig
{
    public int? MaxAge { get; set; } = 60 * 30;
    public int? StaleWhileRevalidate { get; set; } = 60 * 60 * 24 * 7;
    /// <summary>
    /// Allows the user to override the Cache-Control max-age by using a query parameter on the referer level.
    /// Intended for debugging.
    /// </summary>
    public string? RefererMaxAgeOverrideParameterName = "_l10ntsMaxAge";
    public Action<HttpContext>? OnRedirect { get; set; }
}
