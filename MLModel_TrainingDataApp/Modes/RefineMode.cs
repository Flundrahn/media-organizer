using System.Text.Json;
using MLModel_TrainingDataApp.Models;

namespace MLModel_TrainingDataApp.Modes;

public static class RefineMode
{
    public static void Execute(string dataFolder, string outputFile)
    {
        var fileWriter = new FileSystemTrainingDataFileWriter(dataFolder);
        var pipeline = new RefinementPipeline(fileWriter);

        Console.WriteLine("=== Running data refinement pipeline ===\n");

        var raw = File.ReadLines(outputFile)
            .Select(line => JsonSerializer.Deserialize<TrainingEntry>(line)
                            ?? throw new InvalidOperationException($"Failed to deserialize TrainingEntry:\n{line}"))
            .Where(e => e != null)
            .ToList();

        Console.WriteLine($"Loaded {raw.Count} raw entries\n");

        var result = pipeline.Process(raw);

        DisplayCompletion(result, dataFolder);
    }

    private static void DisplayCompletion(RefinementResult result, string dataFolder)
    {
        Console.WriteLine("\n=== Refinement complete ===");
        Console.WriteLine($"Training data (TSV):       {result.TsvPath}");
        Console.WriteLine($"Training data (NER/BIO):   {result.NerPath}");
        Console.WriteLine($"Entity key map:            {Path.Combine(dataFolder, "entity_key_map.txt")}");
        Console.WriteLine($"Filtered data saved in:    {dataFolder}");
        Console.WriteLine("\nReady for Visual Studio NER Model Builder!");
        Console.WriteLine($"  1. Use training file: {result.NerPath}");
        Console.WriteLine($"  2. Entity key map:    entity_key_map.txt");
    }
}
