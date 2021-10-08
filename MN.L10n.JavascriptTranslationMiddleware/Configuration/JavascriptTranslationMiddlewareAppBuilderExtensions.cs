using Microsoft.AspNetCore.Builder;

namespace MN.L10n.JavascriptTranslationMiddleware
{
    public static class JavascriptTranslationMiddlewareAppBuilderExtensions
    {
        /// <summary>
        /// Should be run before UseStaticFiles
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseL10nJavascriptTranslationMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<JavascriptTranslationMiddleware>();
        }
    }
}
