using System;
using Microsoft.Extensions.DependencyInjection;

namespace MN.L10n.JavascriptTranslationMiddleware
{
    public static class JavascriptTranslationMiddlewareServiceCollectionExtensions
    {
        public static IServiceCollection AddL10nJavascriptTranslationMiddleware(this IServiceCollection services,
            string compiledFolder, Action<IJavascriptTranslationMiddlewareConfigurator> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var configurator = new JavascriptTranslationMiddlewareConfigurator(compiledFolder);
            configure(configurator);
            var config = configurator.Build();

            return
                services.AddSingleton(config)
                    .AddSingleton<IJavascriptTranslationL10nLanguageProvider, JavascriptTranslationL10NLanguageProvider
                    >()
                    .AddSingleton<ITranslatorProvider, TranslatorProvider>()
                    .AddScoped<JavascriptTranslationMiddleware>();
        }
    }
}
