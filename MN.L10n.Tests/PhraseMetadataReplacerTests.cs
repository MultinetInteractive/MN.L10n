using MN.L10n.PhraseMetadata;
using Xunit;

namespace MN.L10n.Tests;

public class PhraseMetadataReplacerTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("Deltagare", "Deltagare")]
    [InlineData("Deltagare !ctx=1", "Deltagare")]
    [InlineData("Deltagare !Ctx=1", "Deltagare")]
    [InlineData("Deltagare!ctx=1", "Deltagare!ctx=1")]
    [InlineData("Det finns en massa deltagare !ctx=1 all this text is meta", "Det finns en massa deltagare")]
    public void ReplacesMetadataCorrectly(string phrase, string expected)
    {
        var result = PhraseMetadataReplacer.ReplaceMetadata(phrase);
        Assert.Equal(expected, result);
    }
}
