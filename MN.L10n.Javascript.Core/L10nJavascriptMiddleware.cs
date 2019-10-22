using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;

namespace MN.L10n.Javascript.Core
{
    public class L10nJavascriptMiddleware : IMiddleware
    {
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.Response.ContentType = "text/javascript; charset=utf8";

            context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromHours(1)
            };

            var minifiedScript = context.Request.Query["minified"] == "1";

            var response = RuleEvaluatorFactory.CreateJavascriptRuleEvaluator(minifiedScript);
            return context.Response.WriteAsync(response);
        }
    }
}
