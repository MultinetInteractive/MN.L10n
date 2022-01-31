using Microsoft.AspNetCore.Builder;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace MN.L10n.Javascript.Core
{
    public static class MiddlewareExtensions
    {
        public static IServiceCollection AddL10nJavascriptMiddleware(this IServiceCollection services,
            bool includeTranslations)
        {
            return services
                .AddSingleton<IL10nJavascriptMiddlewareConfig>(new L10nJavascriptMiddlewareConfig
                {
                    IncludeTranslations = includeTranslations
                })
                .AddScoped<L10nJavascriptMiddleware>();
        }

        /// <summary>
        /// Uses the L10nJavascriptMiddleware at the default path : /L10nLanguage.js
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseL10nJavascriptMiddleware(this IApplicationBuilder builder,
            bool includeTranslations = true)
        {
            return UseL10nJavascriptMiddleware(builder, null, includeTranslations);
        }

        public static IApplicationBuilder UseL10nJavascriptMiddleware(this IApplicationBuilder builder,
            Action<L10nJavascriptMiddlewareOptions> configure, bool includeTranslations = true)
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
