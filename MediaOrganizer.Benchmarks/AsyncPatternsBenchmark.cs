using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace MediaOrganizer.Benchmarks;

public class AsyncApiConfig : ManualConfig
{
    public AsyncApiConfig()
    {
        AddJob(Job.Default.WithLaunchCount(1)
                          .WithWarmupCount(3)
                          .WithIterationCount(64)
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

// | Method                                   | Mean      | Error    | StdDev    | Ratio | RatioSD | Completed Work Items | Lock Contentions | Allocated | Alloc Ratio |
// |----------------------------------------- |----------:|---------:|----------:|------:|--------:|---------------------:|-----------------:|----------:|------------:|
// | TaskWhenAllPattern                       |  86.04 ms | 8.739 ms | 20.079 ms |  1.71 |    0.42 |              13.0000 |                - |  50.19 KB |        1.01 |
// | SequentialAwaitPattern                   |  50.62 ms | 2.020 ms |  4.435 ms |  1.01 |    0.12 |              12.6250 |                - |  49.54 KB |        1.00 |
// | SequentialAwaitPatternWithDefaultAwaiter |  42.13 ms | 0.664 ms |  1.486 ms |  0.84 |    0.08 |              14.6667 |                - |  52.07 KB |        1.05 |
// | WhenAnyPattern                           |  43.05 ms | 0.763 ms |  1.722 ms |  0.86 |    0.08 |              13.6000 |                - |   53.5 KB |        1.08 |
// | ThrottledConcurrentPattern               | 127.21 ms | 1.522 ms |  3.466 ms |  2.53 |    0.22 |              22.5000 |                - |  50.66 KB |        1.02 |
// | TrueSequentialPattern                    | 432.98 ms | 2.984 ms |  6.294 ms |  8.62 |    0.72 |              14.2500 |                - |  48.54 KB |        0.98 |

// CONCLUSION: Large variance due to use of real API. 
// 1. Starting tasks all in a row has large impact, 'TrueSequentialPattern' is inefficient as expected.
// 2. No difference in allocated memory.
// 3. ConfigAwait(false) does not seem to have any positive effect here although barely any negative effect either so probably has good uses in other scenarios.
// 4. Starting all tasks then doing sequential await, or WhenAny are equally fast in these measurements, reasonably WhenAny should be a good choice if there are many tasks and some of them run slower than others.
// BEST CHOICE: Since WhenAny does not have any other drawback, and there reasonably are use cases when it is better it should be best choice.

[Config(typeof(AsyncApiConfig))]
public class AsyncPatternsBenchmark
{
    private HttpClient _httpClient = null!;
    private string[] _apiCalls = null!;
    private const int ApiCallCount = 12; // Increased to ensure >100ms iterations

    [GlobalSetup]
    public void Setup()
    {
        _httpClient = new HttpClient();
        // MinimalApi with default weatherforecast implementation + Thread.Sleep of 15 ms to simulate work.
        var baseUrl = "https://luncloud.azurewebsites.net/weatherforecast";
        _apiCalls = Enumerable.Repeat(baseUrl, ApiCallCount).ToArray();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _httpClient?.Dispose();
    }

    [Benchmark]
    public async Task<int> TaskWhenAllPattern()
    {
        var tasks = new Task<WeatherForecast[]?>[_apiCalls.Length];
        for (int i = 0; i < _apiCalls.Length; i++)
        {
            tasks[i] = _httpClient.GetFromJsonAsync<WeatherForecast[]>(_apiCalls[i]);
        }

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.Length;
    }

    [Benchmark(Baseline = true)]
    public async Task<int> SequentialAwaitPattern()
    {
        var tasks = new Task<WeatherForecast[]?>[_apiCalls.Length];
        for (int i = 0; i < _apiCalls.Length; i++)
        {
            tasks[i] = _httpClient.GetFromJsonAsync<WeatherForecast[]>(_apiCalls[i]);
        }

        var results = new List<WeatherForecast[]?>();
        foreach (var task in tasks)
        {
            var result = await task.ConfigureAwait(false);
            results.Add(result);
        }
        return results.Count;
    }

    [Benchmark]
    public async Task<int> SequentialAwaitPatternWithDefaultAwaiter()
    {
        var tasks = new Task<WeatherForecast[]?>[_apiCalls.Length];
        for (int i = 0; i < _apiCalls.Length; i++)
        {
            tasks[i] = _httpClient.GetFromJsonAsync<WeatherForecast[]>(_apiCalls[i]);
        }

        var results = new List<WeatherForecast[]?>();
        foreach (var task in tasks)
        {
            var result = await task;
            results.Add(result);
        }
        return results.Count;
    }

    [Benchmark]
    public async Task<int> WhenAnyPattern()
    {
        var tasks = new List<Task<WeatherForecast[]?>>(_apiCalls.Length);
        for (int i = 0; i < _apiCalls.Length; i++)
        {
            tasks.Add(_httpClient.GetFromJsonAsync<WeatherForecast[]>(_apiCalls[i]));
        }

        var results = new List<WeatherForecast[]?>(_apiCalls.Length);
        while (tasks.Count > 0)
        {
            var finished = await Task.WhenAny(tasks);
            results.Add(await finished);
            tasks.Remove(finished);
        }
        return results.Count;
    }

    [Benchmark]
    public async Task<int> ThrottledConcurrentPattern()
    {
        using var semaphore = new SemaphoreSlim(3, 3);
        var tasks = new Task<WeatherForecast[]?>[_apiCalls.Length];

        for (int i = 0; i < _apiCalls.Length; i++)
        {
            var url = _apiCalls[i];
            tasks[i] = ProcessWithSemaphoreAsync(url, semaphore);
        }

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.Length;
    }

    private async Task<WeatherForecast[]?> ProcessWithSemaphoreAsync(string url, SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return await _httpClient.GetFromJsonAsync<WeatherForecast[]>(url).ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
        }
    }

    [Benchmark]
    public async Task<int> TrueSequentialPattern()
    {
        var results = new List<WeatherForecast[]?>();
        foreach (var url in _apiCalls)
        {
            var response = await _httpClient.GetFromJsonAsync<WeatherForecast[]>(url).ConfigureAwait(false);
            results.Add(response);
        }
        return results.Count;
    }
}

public class WeatherForecast
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("temperatureC")]
    public int TemperatureC { get; set; }

    [JsonPropertyName("temperatureF")]
    public int TemperatureF { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
}