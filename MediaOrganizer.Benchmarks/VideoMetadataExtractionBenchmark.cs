using System.IO.Abstractions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using MediaOrganizer.Benchmarks.Services;
using Perfolizer.Horology;

namespace MediaOrganizer.Benchmarks;

// RESULT 

// | Method                 | Mean      | Error     | StdDev     | Ratio | RatioSD | Allocated | Alloc Ratio |
// |----------------------- |----------:|----------:|-----------:|------:|--------:|----------:|------------:|
// | FFMpeg_HeightOnly      | 78.955 ms | 6.1382 ms | 10.5881 ms |  1.02 |    0.19 |  150.1 KB |        1.00 |
// | MediaInfo_HeightOnly   |  8.497 ms | 0.5709 ms |  0.9848 ms |  0.11 |    0.02 |   2.45 KB |        0.02 |
// | FFMpeg_FullMetadata    | 72.386 ms | 2.2390 ms |  3.8622 ms |  0.93 |    0.13 | 152.58 KB |        1.02 |
// | MediaInfo_FullMetadata |  6.788 ms | 0.0824 ms |  0.1443 ms |  0.09 |    0.01 |   2.82 KB |        0.02 |

// CONCLUSION: MediaInfo is about 10x faster and allocates about 1/50th of the memory,
// does not matter if only fetch info about height or not in either case.

/// <summary>
/// Benchmark comparing FFMpegCore vs MediaInfo for video metadata extraction
/// 
/// IMPORTANT NOTES:
/// 1. This benchmark requires actual video files to test against
/// 2. Results will vary based on file size, codec, and storage speed
/// 3. For I/O heavy operations, consider running with fewer iterations
/// 4. Always use the same test files for consistent comparison
/// </summary>
[Config(typeof(VideoMetadataBenchmarkConfig))]
[MemoryDiagnoser]
public class VideoMetadataExtractionBenchmark
{
    private readonly IFileSystem _fileSystem = new FileSystem();
    private FFMpegVideoMetadataExtractor _ffmpegExtractor = null!;
    private MediaInfoVideoMetadataExtractor _mediaInfoExtractor = null!;

    public string[] TestFiles { get; } = ["D:\\fredr\\Videos\\FourierAnimation.mp4"];

    [GlobalSetup]
    public void Setup()
    {
        Console.WriteLine("🔧 Initializing benchmark setup...");

        _ffmpegExtractor = new FFMpegVideoMetadataExtractor();
        _mediaInfoExtractor = new MediaInfoVideoMetadataExtractor();

        // Validate that FFmpeg extractor is available
        Console.WriteLine($"FFmpeg Available: {_ffmpegExtractor.IsAvailable()}");
        if (!_ffmpegExtractor.IsAvailable())
        {
            throw new InvalidOperationException("FFmpeg/FFprobe not available. Install via: choco install ffmpeg");
        }

        // Validate that MediaInfo extractor is available
        Console.WriteLine($"MediaInfo Available: {_mediaInfoExtractor.IsAvailable()}");
        if (!_mediaInfoExtractor.IsAvailable())
        {
            Console.WriteLine("⚠️  MediaInfo not available - benchmarks will fail");
        }

        // Force garbage collection for consistent starting state
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Console.WriteLine("✅ Benchmark setup completed successfully");
    }

    [IterationSetup]
    public void IterationSetup()
    {
        // Optional: Force GC between iterations for more consistent results
        // Uncomment if multimodal distribution persists
        // GC.Collect();
    }

    // Benchmark 1: Height extraction only (optimized for minimal overhead)
    [Benchmark(Baseline = true)]
    [ArgumentsSource(nameof(TestFiles))]
    public async Task<int> FFMpeg_HeightOnly(string filePath)
    {
        var fileInfo = _fileSystem.FileInfo.New(filePath);
        var r = await _ffmpegExtractor.ExtractHeightOnlyAsync(fileInfo);
        return r;
    }

    // Benchmark 3: MediaInfo height extraction only (optimized)
    [Benchmark]
    [ArgumentsSource(nameof(TestFiles))]
    public async Task<int> MediaInfo_HeightOnly(string filePath)
    {
        var fileInfo = _fileSystem.FileInfo.New(filePath);
        var r = await MediaInfoVideoMetadataExtractor.ExtractHeightOnlyAsync(fileInfo);
        return r;
    }

    // Benchmark 2: Full metadata extraction - TEMPORARILY DISABLED
    [Benchmark]
    [ArgumentsSource(nameof(TestFiles))]
    public async Task<string> FFMpeg_FullMetadata(string filePath)
    {
        var fileInfo = _fileSystem.FileInfo.New(filePath);
        var metadata = await _ffmpegExtractor.ExtractMetadataAsync(fileInfo);
        return $"{metadata.Width}x{metadata.Height}_{metadata.VideoCodec}_{metadata.Duration.TotalSeconds:F1}s";
    }

    // Benchmark 4: MediaInfo full metadata extraction - TEMPORARILY DISABLED
    [Benchmark]
    [ArgumentsSource(nameof(TestFiles))]
    public async Task<string> MediaInfo_FullMetadata(string filePath)
    {
        var fileInfo = _fileSystem.FileInfo.New(filePath);
        var metadata = await _mediaInfoExtractor.ExtractMetadataAsync(fileInfo);
        return $"{metadata.Width}x{metadata.Height}_{metadata.VideoCodec}_{metadata.Duration.TotalSeconds:F1}s";
    }
}

/// <summary>
/// Custom configuration for I/O heavy benchmarks
/// Uses InProcess toolchain to avoid antivirus interference
/// Automatically adjusts invocation count to meet MinIterationTime requirements
/// </summary>
public class VideoMetadataBenchmarkConfig : ManualConfig
{
    public VideoMetadataBenchmarkConfig()
    {
        WithOptions(ConfigOptions.DisableOptimizationsValidator);

        // Hide the specified columns to clean up the output
        HideColumns("Job",
                    "MinIterationTime",
                    "Toolchain",
                    "InvocationCount",
                    "IterationCount",
                    "LaunchCount",
                    "RunStrategy",
                    "WarmupCount",
                    "filePath");

        // Use InProcess toolchain to avoid antivirus interference
        // Set MinIterationTime to automatically adjust invocation count
        AddJob(Job.Default
            .WithRuntime(CoreRuntime.Core90)
            .WithStrategy(RunStrategy.Throughput)
            .WithToolchain(InProcessEmitToolchain.Instance)
            .WithLaunchCount(2)         // Multiple launches to detect consistent patterns
            .WithWarmupCount(5)         // Increased warmup to stabilize performance
            .WithIterationCount(20)     // More iterations for better statistical analysis
            .WithUnrollFactor(1)        // Keep simple for I/O operations
            .WithMinIterationTime(TimeInterval.FromMilliseconds(100))); // Auto-adjust to meet 100ms minimum
    }
}