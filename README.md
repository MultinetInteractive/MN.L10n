# MN.L10n [![Build status](https://ci.appveyor.com/api/projects/status/y5uh8gvxm29v90rk?svg=true)](https://ci.appveyor.com/project/itssimple/mn-l10n)
Translation-thingy for all our products

You must implement your `IL10nLanguageProvider` yourself. :) (Basically just `string GetLanguage()`)

There's also a custom mvc webview `MN.L10n.Mvc.L10nWebView`.

## Example usage (C#)
```csharp
using MN.L10n.NullProviders;
using MN.L10n.FileProviders;
using static MN.L10n.L10n;

void Main()
{
	var l10n = MN.L10n.L10n.CreateInstance(new NullLanguageProvider("en-GB"), new FileDataProvider(@"C:\temp\phrase"));

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

## Linking javascript
```html
...
<script type="text/javascript" src="<%=
ResolveUrl(
  MN.L10n.Javascript
  .Loader.LoadL10nJavascript(
    "~/path/file.js", 
    (file) => 
    { 
      // Check if a translationfile is available, otherwise default is returned
      return System.IO.File.Exists(Server.MapPath(file)); 
    }
)%>"></script>
...
```

## Global.asax.cs
```csharp
protected void Application_Start(object sender, EventArgs e)
{
  ...
  ViewEngines.Engines.Clear();
#if !DEBUG
  ViewEngines.Engines.Add(new PrecompiledViewEngine());
#endif
  ViewEngines.Engines.Add(new RoslynRazorViewEngine());
  ...
  MN.L10n.L10n.CreateInstance(new IL10nLanguageProvider(), new FileDataProvider(@"C:\temp\phrase"));
  ...
}
```

## web/app.config
```xml
...
<configSections>
  ...
  <section name="stackExchange.precompiler" type="StackExchange.Precompilation.PrecompilerSection, StackExchange.Precompilation.Metaprogramming" />
  ...
</configSections>
<stackExchange.precompiler>
  <modules>
    <add type="MN.L10n.CodeCompilerModule, MN.L10n" />
  </modules>
</stackExchange.precompiler>
...
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

## \*.csproj
```xml
<Project...>
  ...
  <PropertyGroup>
    <SEPrecompilerIncludeRazor>true</SEPrecompilerIncludeRazor>
  </PropertyGroup>
  ...
  <Target Name="AfterBuild">
    <ItemGroup>
      <L10nLanguageFiles Include="$(SolutionDir)phrases.json;$(SolutionDir)language-*.json" />
    </ItemGroup>
    <Message Importance="high" Text="Copying phrase-files from @(L10nLanguageFiles) to $(ProjectDir)App_Data\" />
    <Copy SourceFiles="@(L10nLanguageFiles)" DestinationFolder="$(ProjectDir)App_Data\" />
    <Message Importance="high" Text="Files copied to $(ProjectDir)App_Data\" />
  </Target>
```