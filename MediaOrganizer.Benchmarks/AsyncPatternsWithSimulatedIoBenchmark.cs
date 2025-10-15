using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace MediaOrganizer.Benchmarks;

[Config(typeof(AsyncPatternsWithSimulatedIoBenchmarkConfig))]
public class AsyncPatternsWithSimulatedIoBenchmarkConfig : ManualConfig
{
    public AsyncPatternsWithSimulatedIoBenchmarkConfig()
    {
        AddJob(Job.Default.WithLaunchCount(1)
                          .WithWarmupCount(3)
                          .WithIterationCount(16)
                          .WithUnrollFactor(1));

        AddDiagnoser(MemoryDiagnoser.Default);
        AddDiagnoser(ThreadingDiagnoser.Default);

        WithOptions(ConfigOptions.DisableOptimizationsValidator); // Because of MediaInfoDotNet package

        HideColumns("Job",
                    "MinIterationTime",
                    "Toolchain",
                    "InvocationCount",
                    "IterationCount",
                    "LaunchCount",
                    "RunStrategy",
                    "WarmupCount",
                    "UnrollFactor",
                    "filePath");
    }
}

// | Method                 | Mean      | Error    | StdDev   | Ratio | Completed Work Items | Lock Contentions | Allocated | Alloc Ratio |
// |----------------------- |----------:|---------:|---------:|------:|---------------------:|-----------------:|----------:|------------:|
// | TaskWhenAllPattern     | 139.63 ms | 0.467 ms | 0.459 ms |  1.49 |              10.0000 |                - |   3.96 KB |        0.92 |
// | SequentialAwaitPattern | 140.26 ms | 0.765 ms | 0.751 ms |  1.50 |              10.2500 |                - |   3.53 KB |        0.82 |
// | WhenAnyPattern         |  93.43 ms | 0.342 ms | 0.303 ms |  1.00 |              10.0000 |                - |   4.32 KB |        1.00 |
// | TrueSequentialPattern  | 203.06 ms | 0.603 ms | 0.593 ms |  2.17 |              10.0000 |                - |   3.43 KB |        0.79 |

// CONCLUSION: WhenAny pattern works best, other patterns will stand and await for first task of the first batch to finish before starting any of the next.
// WhenAny allows to start follow up task for the fast 10 ms tasks in the first batch, 
// while the slow 50 ms task in first batch continues to work allowing more efficient concurrent work, while using slightly more allocated memory.

// TaskWhenAll => starts all and takes 50 ms to finish, then one by one starts another 10 ms task x5 => 100 ms (+ 40 ms overhead?)
// SequentialAwaitPattern starts all concurrently, awaits the first 50 ms then one by one starts the follow up, first will have finished when first follow up does
// => 50 + 10x5 = 100 ms (+ 40 ms overhead?)

// TrueSequential 50 + 10x4 + 10x5 + 140 (+ 60 ms overhead?)


[Config(typeof(AsyncPatternsWithSimulatedIoBenchmarkConfig))]
public class AsyncPatternsWithSimulatedIoBenchmark
{
    // number of concurrent tasks to simulate per benchmark invocation
    private const int TaskCount = 5;

    // [Params(10, 20, 40)] // simulated latency in ms
    public List<int> LatencyMs { get; } = new()
    {
        50,
        10,
        10,
        10,
        10,
    };

    public int FollowUpLatencyMs = 10;

    private async Task<string> SimulatedIo(int ms, string resultToConcat)
    {
        // Simulated async IO returning a string payload
        await Task.Delay(ms);
        return resultToConcat + "payload" + ms.ToString();
    }

    private Task<string> SimulatedIo(int ms)
    {
        return SimulatedIo(ms, string.Empty);
    }

    // Removed parameterless SimulatedIo - pick latency explicitly per task

    [Benchmark]
    public async Task<List<string>> TaskWhenAllPattern()
    {
        // First task
        var tasks = new List<Task<string>>(TaskCount);
        for (int i = 0; i < TaskCount; i++)
        {
            var ms = LatencyMs[i];
            tasks.Add(SimulatedIo(ms));
        }

        var results = await Task.WhenAll(tasks);

        // Follow up task needs result of first
        var totalResults = new List<string>(TaskCount);
        for (int i = 0; i < TaskCount; i++)
        {
            totalResults.Add(results[i] + await SimulatedIo(FollowUpLatencyMs));
        }

        return totalResults;
    }

    [Benchmark]
    public async Task<List<string>> SequentialAwaitPattern()
    {
        // First task
        var tasks = new List<Task<string>>(TaskCount);
        for (int i = 0; i < TaskCount; i++)
        {
            var ms = LatencyMs[i];
            tasks.Add(SimulatedIo(ms));
        }

        // Follow up task needs result of first 
        var totalResults = new List<string>(TaskCount);
        for (int i = 0; i < TaskCount; i++)
        {
            var first = await tasks[i];
            var follow = await SimulatedIo(FollowUpLatencyMs, first);
            totalResults.Add(follow);
        }

        return totalResults;
    }

    [Benchmark(Baseline = true)]
    public async Task<List<string>> WhenAnyPattern()
    {
        var tasks = new List<Task<string>>(TaskCount);
        for (int i = 0; i < TaskCount; i++)
        {
            var ms = LatencyMs[i];
            tasks.Add(SimulatedIo(ms));
        }

        var totalResults = new List<string>(TaskCount);
        while (tasks.Count > 0)
        {
            var firstTask = await Task.WhenAny(tasks);
            var first = await firstTask;
            var follow = await SimulatedIo(FollowUpLatencyMs, first);
            totalResults.Add(follow);
            tasks.Remove(firstTask);
        }

        return totalResults;
    }

    [Benchmark]
    public async Task<List<string>> TrueSequentialPattern()
    {
        var totalResults = new List<string>(TaskCount);
        for (int i = 0; i < TaskCount; i++)
        {
            var first = await SimulatedIo(LatencyMs[i]);
            var follow = await SimulatedIo(FollowUpLatencyMs, first);
            totalResults.Add(follow);
        }

        return totalResults;
    }
}