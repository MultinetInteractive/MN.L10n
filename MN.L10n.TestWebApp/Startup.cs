using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MN.L10n.FileProviders;
using MN.L10n.JavascriptTranslationMiddleware;
using MN.L10n.NullProviders;

namespace TheTest
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            MN.L10n.L10n.CreateInstance(new NullLanguageProvider("1"), new FileDataProvider("L10n"));
            services.AddL10nJavascriptTranslationMiddleware("compiled", cfg =>
            {
                cfg.EnableCacheWhen(context => Task.FromResult(context.Request.Query.TryGetValue("cache", out var cacheParam) && cacheParam == "1"));
                cfg.AddPathPrefix("/compiled");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseL10nJavascriptTranslationMiddleware();
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Hello World!"); });
            });
        }
    }
}
