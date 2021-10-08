using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace MN.L10n.JavascriptTranslationMiddleware
{
    public class JavascriptTranslationMiddleware : IMiddleware
    {
        private readonly IJavascriptTranslationMiddlewareConfiguration _config;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ITranslatorProvider _translatorProvider;

        public JavascriptTranslationMiddleware(IJavascriptTranslationMiddlewareConfiguration config, IWebHostEnvironment webHostEnvironment, ITranslatorProvider translatorProvider)
        {
            _config = config;
            _webHostEnvironment = webHostEnvironment;
            _translatorProvider = translatorProvider;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var requestHandled = await ProcessRequest(context);

            if (!requestHandled)
            {
                await next(context);
            }
        }
        
        private async Task<bool> ProcessRequest(HttpContext context)
        {
            if (!TryGetRewriteContext(context, out var rewriteContext))
            {
                return false;
            }

            var remainingParts = rewriteContext.Remaining.Value!.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (remainingParts.Length == 0)
            {
                return false;
            }

            var languageId = remainingParts[0];
            var diskPath = string.Join("/", remainingParts.Skip(1));

            var fileHandle = GetFileHandle(diskPath);
            if (!fileHandle.Exists)
            {
                return false;
            }

            var isSupportedFileType = fileHandle.FileName.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase);
            
            if (!isSupportedFileType || !await _config.ShouldTranslateAsync(context, fileHandle))
            {
                var path = string.Join('/', rewriteContext.MatchingSegment, diskPath);
                context.Request.Path = path;
                
                return false;
            }
                
            var translator = _translatorProvider.GetOrCreateTranslator(languageId);
            context.Response.GetTypedHeaders().ContentType = new MediaTypeHeaderValue("text/javascript")
            {
                Encoding = Encoding.UTF8,
                Charset = "utf-8"
            };

            var enableCache = await _config.EnableCacheAsync(context);
            _config.OnBeforeResponse?.Invoke(this, context);
            var translatedContents = await translator.TranslateFileContentsAsync(fileHandle, enableCache);
            await context.Response.WriteAsync(translatedContents);

            return true;
        }
        
        private bool TryGetRewriteContext(HttpContext context, [NotNullWhen(true)] out RewriteContext? rewriteContext)
        {
            foreach (var prefix in _config.RequestPathPrefixes)
            {
                if (context.Request.Path.StartsWithSegments(prefix, out var remaining))
                {
                    rewriteContext = new RewriteContext
                    {
                        MatchingSegment = prefix,
                        Remaining = remaining
                    };

                    return true;
                }
            }

            rewriteContext = null;
            return false;
        }
        
        private class RewriteContext
        {
            public PathString MatchingSegment { get; set; }
            public PathString Remaining { get; set; }
        }

        private FileHandle GetFileHandle(string filePath)
        {
            var fileInfo = _webHostEnvironment.WebRootFileProvider.GetFileInfo(Path.Combine(_config.CompiledFolder, filePath.TrimStart('/')));
            return new FileHandle(fileInfo, filePath);
        }
    }
}
