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
		new FileDataProvider(@"C:\temp\phrase")
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

These are the methods available:

- `_s(string phrase, object args = null)`: Normal string for translation
- `_sr(string phrase, object args = null)`: Same as above, but for output in MVC/Razor (Returns `IHtmlContent`)
- `_m(string phrase, object args = null)`: Markdown-string for translation
- `_mr(string phrase, object args = null)`: Same as above, but for output in MVC/Razor (Returns `IHtmlContent`)

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
  MN.L10n.L10n.CreateInstance(new IL10nLanguageProvider(), new FileDataProvider(@"C:\temp\phrase"));
  ...
}
```

# L10n and you, a guide for translations and localization

This part of the documentation will provide information for you to know:

- [How to install `MN.L10n`](#how-to-install-mnl10n)
- [How to get `MN.L10n.BuildTasks` to work](#how-to-get-mnl10nbuildtasks-to-work)
- [How to properly write phrases to be translated and pluralized](#how-to-properly-write-phrases-to-be-translated-and-pluralized)
- [What is the `.l10nconfig`-thing?](#what-is-the-l10nconfig-thing)
- [How do I `languages.json`?](#how-do-i-languagesjson)

## How to install `MN.L10n`

To be able to use `MN.L10n`, you have to install the NuGet-package from our Artifactory into all projects where you want to be able to use it for translation.

The best way to use it, is to actually add it as `using static MN.L10n.L10n;` in your usings, because then you can use `_s` and `_m` directly, without having to prefix it with `L10n._s`.

And to be able to get the files for translation, you also need to install `MN.L10n.BuildTasks`, into one or more projects, depending on how many projects you want different "projects" for in i.e. GlotPress.

And then, to actually be able to use it, you have to create an instance of it.

```csharp
MN.L10n.L10n.CreateInstance(
  new NullLanguageProvider("1"), // 1 in this case is the source language
  new FileDataProvider(@"C:\temp\phrase") // This is the path where you load the language from
);
```

There are different providers for different things, and most projects have their own `LanguageProvider`.

These are the methods available for translation:

- `_s(string phrase, object args = null)`: Normal string for translation
- `_sr(string phrase, object args = null)`: Same as above, but for output in MVC/Razor (Returns `IHtmlContent`)
- `_m(string phrase, object args = null)`: Markdown-string for translation
- `_mr(string phrase, object args = null)`: Same as above, but for output in MVC/Razor (Returns `IHtmlContent`)

---

## How to get `MN.L10n.BuildTasks` to work

First of all, you need to install the NuGet-package `MN.L10n.BuildTask` in at least *one* project.

Currently, it will only run while the solution is being compiled in `Release`-config, so that we don't slow down local build times too much.

When the solution is run in `Release`-mode, L10n will look for either a `.l10nconfig` or the `.sln`-file, as the root for where it should look for phrases.

---

## How to properly write phrases to be translated and pluralized

So, you want to use L10n the way you're supposed to? Awesome!

First of all, you have to decide what language is the **Source Language**. So, no mixing languages.

### **Pluralization**

First, we'll go over what you **shouldn't** do, because I've seen this a lot.

```csharp
if(numberVariable == 1) {
 _s("$__count$ thing", new { __count = numberVariable });
} else {
 _s("$__count$ things", new { __count = numberVariable });
}
```

Because this will register as two separate phrases that will need pluralization in GlotPress.

Instead, do it like this, because it will only register once by L10n, but still be pluralizable in GlotPress.

```csharp
_s("$__count$ things", new { __count = numberVariable });
```

If you didn't notice from the examples, the magic property is called `$__count$`,
please stop using your own names for variables that need pluralization, it won't work..

---

### **Line breaks**

You handle line breaks by using any type of string, that supports new lines in them.

Javascript/TypeScript

```javascript
_s(`This
is
SPARTA`);
```

C#

```csharp
_s(@"This
is
SPARTA");
```

Don't _ever_ use string concatenation, I do **not** want to see anything like this

```csharp
_s("This" + Environment.NewLine + "is" + Environment.NewLine + "SPARTA");
```

---

### **Parameters/Variables**

I don't remember right now that I have seen anyone use this the wrong way, but better safe than sorry.

Don't use string literals (strings with variables in it), it might work, but it's not intentional in that case.

Javascript/TypeScript

```javascript
_s(`This ${isWord} not allowed, it is not intentionally supported, if it even works`);
```

C#

```csharp
_s($"This {isWord} not allowed, it is not intentionally supported, if it even works");
```

Instead, if you want to use variables, the correct way for L10n, use it like this (bad example, but still)

```csharp
_s(
 "This $isWord$ not allowed, it is not intentionally supported, if it even works",
 new { isWord = "is" }
);
```

Reserved variables

- `$__count$` / `__count`, used for pluralization.

---

### **Dynamic phrases**

Normally, L10n *does not* allow dynamic phrases (you'll get those warnings from the `MN.L10n.Analyzer`, when you have it installed).

So, I won't be going into this here. L10n is not made for dynamic phrases. :smile:

---

## What is the `.l10nconfig`-thing

Example config

```jsonc
{
 /*
  * IncludePatterns is an array that
  * will make sure that you won't miss
  * any packages in i.e. node_modules
  */
 "IncludePatterns": [
  "\\@multinet\\",
  "/@multinet/"
  ],

 /*
  * ExcludePatterns is an array that
  * will ignore some paths/folders,
  * so that you won't get double instances
  * phrases from compiled versions of the code
  */
 "ExcludePatterns": [
  "\\compiled\\"
  ],

 /* This will make the log, really verbose */
 "ShowDetailedLog": false,

 /* Well, it should be very obvious what this one does */
 "PreventBuildTask": false,

 /*
  * Setting this to true, will make the BuildTask
  * download the phrases from the sources in languages.json
  */
 "DownloadTranslationFromSourcesOnBuild": true,

 /*
  * An array of what directories L10n should
  * copy all it's compiled files to
  */
 "CopyFilesTo": [
  "sample-folder",
  "sample-folder2"
 ],

 /* This is the language the app is written in */
 "SourceLanguage": "1"
}
```

---

## How do I `languages.json`

Example config

```jsonc
[
 {
  /* This should map to the language identifier in your application */
  "LanguageId": "1",

  /* Sources contains an array of URLs from where we should download translations on build */
  "Sources": [
  ]
 }
]
```

---

#### Things we use in the code to make the magic happen
- [CommonMark.NET](https://github.com/Knagis/CommonMark.NET)
- [NGettext](https://github.com/neris/NGettext/)
