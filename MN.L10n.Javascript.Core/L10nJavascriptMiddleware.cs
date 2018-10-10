using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;

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

            var cache = context.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
            var minifiedScript = context.Request.Query["minified"] == "1";

            var response = RuleEvaluatorFactory.CreateJavascriptRuleEvaluator(cache, minifiedScript);
            return context.Response.WriteAsync(response);
        }
    }
}
