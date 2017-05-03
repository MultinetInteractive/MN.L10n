# MN.L10n
Translation-thingy for all our products

Install the nuget-package `MN.L10n`.

_More info will come, when I've actually coded some. :)_

## Example usage
```csharp
using MN.L10n.NullProviders;
using static MN.L10n.L10n;

void Main()
{
	var l10n = MN.L10n.L10n.CreateInstance(
	   new NullLanguageProvider(), 
	   new NullDataProvider()
	);

	Console.WriteLine(
	   _s(
	      "Det finns $__count$ meddelanden", // Det finns $__count$ meddelanden
	      new { __count = 10 })
	);
	Console.WriteLine(
	   _m("[Hejsan](Text)") // <p><a href="Text">Hejsan</a></p>
	);
}
```