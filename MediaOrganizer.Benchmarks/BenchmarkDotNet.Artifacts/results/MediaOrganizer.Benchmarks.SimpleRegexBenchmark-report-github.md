```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26100.6584/24H2/2024Update/HudsonValley)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.305
  [Host]     : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3


```
| Method                    | Mean       | Error    | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |-----------:|---------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| NewRegexEachTimeBenchmark | 4,420.9 ns | 87.20 ns | 133.17 ns |  1.00 |    0.04 | 1.6632 | 0.0076 |    7848 B |        1.00 |
| StaticRegexBenchmark      |   303.1 ns |  6.03 ns |   6.45 ns |  0.07 |    0.00 |      - |      - |         - |        0.00 |
| CompiledRegexBenchmark    |   147.7 ns |  2.91 ns |   3.11 ns |  0.03 |    0.00 |      - |      - |         - |        0.00 |
| GeneratedRegexBenchmark   |   142.1 ns |  2.86 ns |   4.36 ns |  0.03 |    0.00 |      - |      - |         - |        0.00 |
