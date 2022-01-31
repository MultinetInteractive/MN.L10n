using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MN.L10n.JavascriptTranslationMiddleware
{
    public interface IJavascriptTranslationMiddlewareConfigurator
    {
        /// <summary>
        /// Add a path prefix for which the plugin should run. For example if you add /plugin the plugin will run for all requests starting with /plugin
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        IJavascriptTranslationMiddlewareConfigurator AddPathPrefix(PathString prefix);
        /// <summary>
        /// Use to set when the translations should be cached, by default it always is
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IJavascriptTranslationMiddlewareConfigurator EnableCacheWhen(Func<HttpContext, Task<bool>> predicate);

        /// <summary>
        /// Use to configure when the translationmiddleware should translate JS files. By default it always does.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IJavascriptTranslationMiddlewareConfigurator TranslateWhen(Func<HttpContext, FileHandle, Task<bool>>? predicate);
    }

    internal class JavascriptTranslationMiddlewareConfigurator : IJavascriptTranslationMiddlewareConfigurator
    {
        private readonly List<PathString> _pathPrefixes = new();
        private readonly string _compiledFolder;
        private Func<HttpContext, FileHandle, Task<bool>>? _shouldTranslateAsync;
        private Func<HttpContext, Task<bool>>? _shouldEnableCacheAsync;

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

        public IJavascriptTranslationMiddlewareConfiguration Build()
        {
            if (string.IsNullOrWhiteSpace(_compiledFolder)) throw new ArgumentException("Missing compiled folder");

            if (_pathPrefixes.Count == 0)
                throw new ArgumentException(
                    $"At least one pathPrefix must be provided, plase call {nameof(AddPathPrefix)} while configuring the {nameof(JavascriptTranslationMiddleware)}");

            var config =
                new JavascriptTranslationMiddlewareConfiguration(_pathPrefixes.ToArray(), _compiledFolder.Trim());
            if (_shouldTranslateAsync is not null) config.ShouldTranslateAsync = _shouldTranslateAsync;

            if (_shouldEnableCacheAsync is not null) config.EnableCacheAsync = _shouldEnableCacheAsync;

            return config;
        }
    }
}
