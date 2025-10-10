using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Text.RegularExpressions;

namespace MediaOrganizer.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<SimpleRegexBenchmark>();
    }
}

// 1 test file string

//| Method                    | Mean        | Error     | StdDev    | Ratio | RatioSD |
//|-------------------------- |------------:|----------:|----------:|------:|--------:|
//| NewRegexEachTimeBenchmark | 1,524.17 ns | 30.315 ns | 47.197 ns |  1.00 |    0.04 |
//| StaticRegexBenchmark      |   120.09 ns |  2.380 ns |  2.923 ns |  0.08 |    0.00 |
//| CompiledRegexBenchmark    |    53.92 ns |  1.087 ns |  1.016 ns |  0.04 |    0.00 |

// 3 test files string

//| Method                    | Mean       | Error    | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
//|-------------------------- |-----------:|---------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
//| NewRegexEachTimeBenchmark | 4,420.9 ns | 87.20 ns | 133.17 ns |  1.00 |    0.04 | 1.6632 | 0.0076 |    7848 B |        1.00 |
//| StaticRegexBenchmark      |   303.1 ns |  6.03 ns |   6.45 ns |  0.07 |    0.00 |      - |      - |         - |        0.00 |
//| CompiledRegexBenchmark    |   147.7 ns |  2.91 ns |   3.11 ns |  0.03 |    0.00 |      - |      - |         - |        0.00 |
//| GeneratedRegexBenchmark   |   142.1 ns |  2.86 ns |   4.36 ns |  0.03 |    0.00 |      - |      - |         - |        0.00 |

// CONCLUSION: Compiled and source generated regex are equally fast to run with zero allocated memory,
// but source generated regex have zero startup cost, and only has cost once at build time, which is nice.

[SimpleJob]
[MemoryDiagnoser]

public partial class SimpleRegexBenchmark
{
    private readonly string[] _testFiles = {
        "Breaking.Bad.S01E01.720p.HDTV.x264-CTU.mkv",
        "The.Office.S02E14.The.Carpet.720p.WEB-DL.DD5.1.H.264-KiNGS.mp4",
        "Game of Thrones (2011) S08E06 The Iron Throne.avi"
    };

    private const string Pattern = @"\.S\d{1,2}E\d{1,2}";

    [GeneratedRegex(Pattern, RegexOptions.IgnoreCase)]
    private static partial Regex GeneratedSeasonEpisodeRegex();

    [Benchmark(Baseline = true)]
    public int NewRegexEachTimeBenchmark()
    {
        int matches = 0;
        foreach (var file in _testFiles)
        {
            var regex = new Regex(Pattern, RegexOptions.IgnoreCase);
            if (regex.IsMatch(file))
                matches++;
        }
        return matches;
    }

    private static readonly Regex StaticRegex = new(Pattern, RegexOptions.IgnoreCase);

    [Benchmark]
    public int StaticRegexBenchmark()
    {
        int matches = 0;
        foreach (var file in _testFiles)
        {
            if (StaticRegex.IsMatch(file))
                matches++;
        }
        return matches;
    }

    private static readonly Regex CompiledRegex = new(Pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

    [Benchmark]
    public int CompiledRegexBenchmark()
    {
        int matches = 0;
        foreach (var file in _testFiles)
        {
            if (CompiledRegex.IsMatch(file))
                matches++;
        }
        return matches;
    }

    [Benchmark]
    public int GeneratedRegexBenchmark()
    {
        int matches = 0;
        foreach (var file in _testFiles)
        {
            if (GeneratedSeasonEpisodeRegex().IsMatch(file))
                matches++;
        }
        return matches;
    }
}