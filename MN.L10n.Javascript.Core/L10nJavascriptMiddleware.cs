using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;

namespace MN.L10n.Javascript.Core
{
    internal class L10nJavascriptMiddleware : IMiddleware
    {
        private readonly IL10nJavascriptMiddlewareConfig _config;

        public L10nJavascriptMiddleware(IL10nJavascriptMiddlewareConfig config)
        {
            _config = config;
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.Response.ContentType = "text/javascript; charset=utf8";

            context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromHours(1)
            };

            var minifiedScript = context.Request.Query["minified"] == "1";

            var response = RuleEvaluatorFactory.CreateJavascriptRuleEvaluator(minifiedScript, _config.IncludeTranslations);
            return context.Response.WriteAsync(response);
        }
    }
}
