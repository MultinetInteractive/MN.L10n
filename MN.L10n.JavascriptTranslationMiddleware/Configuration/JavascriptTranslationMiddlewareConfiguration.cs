using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MN.L10n.JavascriptTranslationMiddleware
{
    public interface IJavascriptTranslationMiddlewareConfiguration
    {
        PathString[] RequestPathPrefixes { get; }
        string CompiledFolder { get; }
        Func<HttpContext, FileHandle, Task<bool>> ShouldTranslateAsync { get; }
        Func<HttpContext, Task<bool>> EnableCacheAsync { get; }
    }

    internal class JavascriptTranslationMiddlewareConfiguration : IJavascriptTranslationMiddlewareConfiguration
    {
        public PathString[] RequestPathPrefixes { get; }
        public string CompiledFolder { get; }
        public Func<HttpContext, FileHandle, Task<bool>> ShouldTranslateAsync { get; set; } = (_, _) => Task.FromResult(true);
        public Func<HttpContext, Task<bool>> EnableCacheAsync { get; set; } = _ => Task.FromResult(true);

        public JavascriptTranslationMiddlewareConfiguration(PathString[] requestPathPrefixes, string compiledFolder)
        {
            RequestPathPrefixes = requestPathPrefixes;
            CompiledFolder = compiledFolder;
        }
    }
}
