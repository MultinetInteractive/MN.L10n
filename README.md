# MN.L10n
Översättnings-bibliotek för alla tjänster!

Dra ner nuget-paketet `mn.l10n`.

_Mer info kommer så fort jag faktiskt kodat lite. :)_

## Exempelanvändning
```csharp
void Main()
{
	var l10n = MN.L10n.L10n.CreateInstance(
	   new NullLanguageProvider(), 
	   new NullDataProvider()
	);

	Console.WriteLine(
	   _s(
	      "Det finns $__count$ meddelanden", 
	      new { __count = 10 })
	);
	Console.WriteLine(
	   _m("[Hejsan](Text)")
	);
}
```