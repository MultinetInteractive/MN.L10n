# MN.L10n
Translation-thingy for all our products

Install the nuget-package `MN.L10n`.

_More info will come, when I've actually coded some. :)_

See [phrases.json](http://phoenix.net.multinet.se/general/mn-l10n/snippets/5), 
[language-sv-SE.json](http://phoenix.net.multinet.se/general/mn-l10n/snippets/6),
[language-en-GB.json](http://phoenix.net.multinet.se/general/mn-l10n/snippets/7) for json format

## Example usage
```csharp
using MN.L10n.NullProviders;
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