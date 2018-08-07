using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace MN.L10n.Javascript.Core
{
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Uses the L10nJavascriptMiddleware at the default path : /L10nLanguage.js
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseL10nJavascriptMiddleware(this IApplicationBuilder builder)
        {
            return UseL10nJavascriptMiddleware(builder, null);
        }

        public static IApplicationBuilder UseL10nJavascriptMiddleware(this IApplicationBuilder builder, Action<L10nJavascriptMiddlewareOptions> configure)
        {
            var opts = new L10nJavascriptMiddlewareOptions();
            configure?.Invoke(opts);

            return builder.UseWhen(
                context => 
                context.Request.Path.HasValue && context.Request.Path.Value == opts.Url,
                innerBuilder => innerBuilder.UseMiddleware<L10nJavascriptMiddleware>()
            );
        }
    }
}
