using BenchmarkDotNet.Running;

namespace MediaOrganizer.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Media Organizer - Performance Testing"
                          + "=====================================\n"
                          + "💡 Available benchmark modes:\n"
                          + "   dotnet run metadataextraction --configuration Release - Scientific video benchmark\n"
                          + "   dotnet run regex --configuration Release     - Regex performance benchmark\n"
                          + "   dotnet run asyncpatterns --configuration Release     - Async patterns with API call performance benchmark\n");

        if (args.Length > 0 && args[0].Equals("metadataextraction", StringComparison.OrdinalIgnoreCase))
        {
            BenchmarkRunner.Run<VideoMetadataExtractionBenchmark>();
            return;
        }
        
        if (args.Length > 0 && args[0].Equals("regex", StringComparison.OrdinalIgnoreCase))
        {
            BenchmarkRunner.Run<SimpleRegexBenchmark>();
            return;
        }

        if (args.Length > 0 && args[0].Equals("asyncpatterns", StringComparison.OrdinalIgnoreCase))
        {
            BenchmarkRunner.Run<AsyncPatternsBenchmark>();
            return;
        }
    }
}
