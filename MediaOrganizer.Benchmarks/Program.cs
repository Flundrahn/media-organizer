using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MediaOrganizer.Benchmarks.Services;
using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace MediaOrganizer.Benchmarks;

public class Program
{
    public static async Task Main(string[] args)
    {
        // string filePath = "D:\\fredr\\Videos\\FourierAnimation.mp4";
        // var fileInfo = new FileSystem().FileInfo.New(filePath);
        // int result = await MediaInfoVideoMetadataExtractor.ExtractHeightOnlyAsync(fileInfo);
        // Console.WriteLine("Result of resolution issss: " + result + "");

        Console.WriteLine("Media Organizer - Performance Testing"
                          + "=====================================\n"
                          + "💡 Available benchmark modes:\n"
                          + "   dotnet run                                   - Practical video metadata test\n"
                          + "   dotnet run benchmark --configuration Release - Scientific video benchmark\n"
                          + "   dotnet run regex --configuration Release     - Regex performance benchmark\n");

        // Check for benchmark argument
        if (args.Length > 0 && args[0].Equals("benchmark", StringComparison.OrdinalIgnoreCase))
        {
            BenchmarkRunner.Run<VideoMetadataExtractionBenchmark>();
            return;
        }
        
        // Check for regex benchmark argument
        if (args.Length > 0 && args[0].Equals("regex", StringComparison.OrdinalIgnoreCase))
        {
            BenchmarkRunner.Run<SimpleRegexBenchmark>();
            return;
        }
    }
}
