# MN.L10n [![Build status](https://ci.appveyor.com/api/projects/status/y5uh8gvxm29v90rk?svg=true)](https://ci.appveyor.com/project/itssimple/mn-l10n) [![Gitter chat](https://badges.gitter.im/MultinetInteractive/MN.L10n.png)](https://gitter.im/MultinetInteractive/MN.L10n)
Translation-thingy for all our products

You must implement your `IL10nLanguageProvider` and a custom `IFileResolver` (for javascript) yourself. :) (Basically just `string GetLanguage()` and `bool FileExists(string file)`)

There's also a custom mvc webview `MN.L10n.Mvc.L10nWebView`.

## Example usage (C#)
```csharp
using MN.L10n.NullProviders;
using MN.L10n.FileProviders;
using static MN.L10n.L10n;

void Main()
{
	var l10n = MN.L10n.L10n.CreateInstance(
		new NullLanguageProvider("en-GB"), 
		new FileDataProvider(@"C:\temp\phrase"), 
		new FileResolver()
	);

	Console.WriteLine(
		_s("Det finns $__count$ meddelanden", 
			new { __count = 0 }
		)
	); // There are no messages
	
	Console.WriteLine(
		_s("Det finns $__count$ meddelanden", 
			new { __count = 1 }
		)
	); // There is one message
	
	Console.WriteLine(
		_s("Det finns $__count$ meddelanden", 
			new { __count = 2 }
		)
	); // There are 2 messages
	
	Console.WriteLine(
		_m("[Hejsan $name$](http://www.multinet.se)", 
			new { name = "Anders" }
		)
	); // <p><a href="http://www.multinet.se">Hejsan Anders</a></p>
}
```

## Example usage (Javascript)
First you need to link our javascript into the pages where you want to enable global usage of `_s` and `_m`.
```csharp
<%
Response.Write("<script type=\"text/javascript\">" + 
  MN.L10n.Properties.Resources.L10n + 
"</script>");
%>
```

```javascript
DealDetails.ShowNotification(
  _s('Sparade en ny notering på $companyName$', 
    { companyName: DealDetails.DealInfo.CompanyName }
  )
);
```

## Global.asax.cs
```csharp
protected void Application_Start(object sender, EventArgs e)
{
  ...
  MN.L10n.L10n.CreateInstance(new IL10nLanguageProvider(), new FileDataProvider(@"C:\temp\phrase"), new FileResolver());
  ...
}
```

## web.config (In MVC Views)
```xml
...
<system.web.webPages.razor>
  ...
  <pages pageBaseType="MN.L10n.Mvc.L10nWebView">
    <namespaces>
      ...
      <add namespace="MN.L10n"/>
      ...
    </namespaces>
  </pages>
</system.web.webPages.razor>
...
```

---

# Example files

## languages.json

```json
[ "sv_SE", "en_GB" ]
```

## phrases.json

```json
{ 
  Phrases: { 
    "Hello world": { 
      "Created": "2017-01-01T00:00:00.000Z",
      "Usages": 1,
      "Comment": "Standard phrase, found everywhere."
    }
  }
}
```

## language-sv_SE.json

```json
{
  "LanguageName": "Svenska",
  "Locale": "sv-SE",
  "PluralizationRules": [ "0", "1" ],
  "PluralRule": "(n != 1)",
  "Phrases": {
	"Hello world": {
	  "r": {
		"0": "Hej världen"
	  }
	}
  }
}
```
---

#### Things we use in the code to make the magic happen
- [Glob.cs](https://github.com/mganss/Glob.cs)
- [CommonMark.NET](https://github.com/Knagis/CommonMark.NET)
- [Jil](https://github.com/kevin-montrose/Jil)
- [NGettext](https://github.com/neris/NGettext/)
