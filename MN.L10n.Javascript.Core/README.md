# Usage: add the following to Startup.cs

```csharp
services.AddSingleton<L10nJavascriptMiddleware>();

app.UseL10nJavascriptMiddleware();
//or app.UseL10nJavascriptMiddleware(cfg => cfg.Url = "/l10n/language.js")
```
