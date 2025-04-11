// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MN.L10n;
using MN.L10n.FileProviders;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;




BenchmarkRunner.Run<Benchmarks>();




public class BenchmarkL10nLanguageProvider : IL10nLanguageProvider
{
    public string GetLanguage()
    {
        return "1";
    }
}

public class BenchmarkL10nDataProvider : IL10nDataProvider
{
    private L10n _l10n = new();
    public L10n LoadL10n()
    {
        return _l10n;
    }

    public Task<bool> LoadTranslationFromSources(L10n l10n, bool removeAllPhrases, CancellationToken token)
    {
        return Task.FromResult(true);
    }

    public bool SaveL10n(L10n l10n)
    {
        _l10n = l10n;
        return true;
    }

    public bool SaveTranslation(L10n l10n)
    {
        return true;
    }
}


public class Foo
{
    public string data { get; set; } = "";
}

[MemoryDiagnoser(true)]
[InvocationCount(100_000)]
public class Benchmarks
{
    [Params("$data$", "Hej $data$ $count$ $many$", "$den här texten inleds med $data$$data2$", "Här har vi en längre förklarande text utan dollartecken. Den här skulle t.ex. kunna finnas i ")]
    public string formatString { get; set; } = "";
    public static Foo args = new Foo { data = "Anders" };

    [GlobalSetup]
    public void GlobalSetup()
    {
        var dataProvider = new BenchmarkL10nDataProvider();

        var stack = new Stack<string>();
        stack.Push("1");
        var items = new Dictionary<object, object>() { { "___l10nlang",  stack } };
        var l10n = L10n.CreateInstance(new BenchmarkL10nLanguageProvider(), new FileDataProvider(AppDomain.CurrentDomain.BaseDirectory), () => items);
        dataProvider.SaveL10n(l10n);
    }

    [Benchmark]
    public void GetPhase()
    {
        L10n._s(formatString, args);
    }

    [Benchmark]
    public void GetPhaseWithoutArgs()
    {
        L10n._s(formatString);
    }

    [Benchmark]
    public void FormatNamed()
    {
        L10n.FormatNamed(formatString, args);
    }

    [Benchmark]
    public void GetLanguage()
    {
        L10n.GetLanguage();
    }
}
