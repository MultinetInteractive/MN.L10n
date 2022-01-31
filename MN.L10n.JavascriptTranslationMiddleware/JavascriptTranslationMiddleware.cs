using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MN.L10n.JavascriptTranslationMiddleware
{
    public class JavascriptTranslationMiddleware : IMiddleware
    {
        private readonly IJavascriptTranslationMiddlewareConfiguration _config;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ITranslatorProvider _translatorProvider;
        private readonly ILogger _logger;

        public JavascriptTranslationMiddleware(IJavascriptTranslationMiddlewareConfiguration config, IWebHostEnvironment webHostEnvironment, ITranslatorProvider translatorProvider, ILogger<JavascriptTranslationMiddleware> logger)
        {
            _config = config;
            _webHostEnvironment = webHostEnvironment;
            _translatorProvider = translatorProvider;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _logger.LogTrace($"Begin invoke at {context.Request.Path}");
            await ProcessRequest(context);
            await next(context);
            _logger.LogTrace("End invoke");
        } 
        
        private async Task ProcessRequest(HttpContext context)
        {
            if (!TryGetRewriteContext(context, out var rewriteContext))
            {
                return;
            }

            var remainingParts = rewriteContext.Remaining.Value!.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (remainingParts.Length == 0)
            {
                return;
            }

            var languageId = remainingParts[0];
            _logger.LogTrace($"LanguageId resolved as {languageId}");
            var diskPath = string.Join("/", remainingParts.Skip(1));

            var fileHandle = GetFileHandle(diskPath);
            if (!fileHandle.Exists)
            {
                _logger.LogDebug($"Unable to resolve file at {fileHandle.Path}");
                return;
            }

            var isSupportedFileType = fileHandle.FileName.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase);

            var translate = true;
            if (!isSupportedFileType)
            {
                translate = false;
                _logger.LogTrace($"file {fileHandle.FileName} has an usupported fileType");
            }
            
            if (!await _config.ShouldTranslateAsync(context, fileHandle))
            {
                translate = false;
                _logger.LogTrace($"file {fileHandle.FileName} was not translated because ShouldTranslateAsync returned false");
            }

            if (!translate)
            {
                var path = string.Join('/', rewriteContext.MatchingSegment, diskPath);
                _logger.LogTrace($"Updated path to be {path}");
                context.Request.Path = path;
                return;
            }
            
            var translator = _translatorProvider.GetOrCreateTranslator(languageId, fileHandle);
            var enableCache = await _config.EnableCacheAsync(context);
            _logger.LogTrace($"Translating file at {fileHandle.Path}, {(enableCache ? "using cache" : "not using cache")}");
            var translatedFileInformation = await translator.TranslateFile(enableCache);
            _logger.LogTrace($"Translated file saved at {translatedFileInformation.FilePath}");

            var newPath = string.Join('/', rewriteContext.MatchingSegment,
                translatedFileInformation.RelativeRequestPath);
            context.Request.Path = newPath;
            _logger.LogTrace($"Updated path to be {newPath}");
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
