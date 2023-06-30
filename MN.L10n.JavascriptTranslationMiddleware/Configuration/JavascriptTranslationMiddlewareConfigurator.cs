using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MN.L10n.JavascriptTranslationMiddleware
{
    public interface IJavascriptTranslationMiddlewareConfigurator
    {
        IJavascriptTranslationMiddlewareConfigurator AddPathPrefix(PathString prefix);
        IJavascriptTranslationMiddlewareConfigurator EnableCacheWhen(Func<HttpContext, Task<bool>> predicate);

        IJavascriptTranslationMiddlewareConfigurator TranslateWhen(
            Func<HttpContext, FileHandle, Task<bool>>? predicate);

        IJavascriptTranslationMiddlewareConfigurator EnableVersionedFileRedirect(Action<VersionedFileRedirectConfig>? configure = null);
        IJavascriptTranslationMiddlewareConfigurator DisableVersionedFileRedirect();
    }

    internal class JavascriptTranslationMiddlewareConfigurator : IJavascriptTranslationMiddlewareConfigurator
    {
        private readonly List<PathString> _pathPrefixes = new();
        private readonly string _compiledFolder;
        private Func<HttpContext, FileHandle, Task<bool>>? _shouldTranslateAsync;
        private Func<HttpContext, Task<bool>>? _shouldEnableCacheAsync;
        private VersionedFileRedirectConfig? _versionedFileRedirectConfig = new();

        public JavascriptTranslationMiddlewareConfigurator(string compiledFolder)
        {
            _compiledFolder = compiledFolder;
        }
        
        public IJavascriptTranslationMiddlewareConfigurator AddPathPrefix(PathString prefix)
        {
            _pathPrefixes.Add(prefix);
            return this;
        }
        public IJavascriptTranslationMiddlewareConfigurator EnableCacheWhen(Func<HttpContext, Task<bool>> predicate)
        {
            _shouldEnableCacheAsync = predicate;
            return this;
        }
        
        public IJavascriptTranslationMiddlewareConfigurator TranslateWhen(
            Func<HttpContext, FileHandle, Task<bool>>? predicate)
        {
            _shouldTranslateAsync = predicate;
            return this;
        }
        
        public IJavascriptTranslationMiddlewareConfigurator EnableVersionedFileRedirect(Action<VersionedFileRedirectConfig>? configure = null)
        {
            _versionedFileRedirectConfig = new VersionedFileRedirectConfig();
            configure?.Invoke(_versionedFileRedirectConfig);
            return this;
        }
        
        public IJavascriptTranslationMiddlewareConfigurator DisableVersionedFileRedirect()
        {
            _versionedFileRedirectConfig = null;
            return this;
        }

        public IJavascriptTranslationMiddlewareConfiguration Build()
        {
            if (string.IsNullOrWhiteSpace(_compiledFolder)) throw new ArgumentException("Missing compiled folder");

            if (_pathPrefixes.Count == 0)
                throw new ArgumentException(
                    $"At least one pathPrefix must be provided, plase call {nameof(AddPathPrefix)} while configuring the {nameof(JavascriptTranslationMiddleware)}");

            var config =
                new JavascriptTranslationMiddlewareConfiguration(_pathPrefixes.ToArray(), _compiledFolder.Trim())
                {
                    VersionedFileRedirectConfig = _versionedFileRedirectConfig
                };
            if (_shouldTranslateAsync is not null) config.ShouldTranslateAsync = _shouldTranslateAsync;

            if (_shouldEnableCacheAsync is not null) config.EnableCacheAsync = _shouldEnableCacheAsync;

            return config;
        }
    }
}
