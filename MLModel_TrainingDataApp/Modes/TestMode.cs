using System.Text.Json;
using MLModel_TrainingDataApp;
using MLModel_TrainingDataApp.Models;

namespace MLModel_TrainingDataApp.Modes;

/// <summary>
/// Test mode: evaluates the trained NER model on sample filenames.
/// </summary>
public static class TestMode
{
    public static void Execute(string outputFile)
    {
        Console.WriteLine("=== Testing Trained NER Model ===\n");

        var testFilenames = new[]
        {
            "The.Day.of.the.Jackal.S01E07.1080p.WEB.H264",
            "Breaking.Bad.S01E01.Pilot.720p.BluRay.x264",
            "Game.of.Thrones.S08E06.The.Iron.Throne.2160p.WEB-DL",
            "Stranger.Things.S04E01.1080p.NF.WEB-DL.DDP5.1.x264",
            "Money.Heist.S03E08.720p.NF.WEB-DL",
            "Law.And.Order.Organized.Crime.S04E09.1080p.WEB.h264",
            "Star.Trek.Voyager.S06E11.Fair.Haven.480p.DVD.x265",
            "Nip.Tuck.S05E02.1080p.WEB-DL",
            "Wild.Wild.West.S04E08.480p",
            "Dawsons.Creek.S04E09.720p"
        };

        ShowNamePredictor.TestOnExamples(testFilenames);

        // Optionally evaluate on validation data
        if (File.Exists(outputFile))
        {
            EvaluateOnValidationData(outputFile);
        }
    }

    private static void EvaluateOnValidationData(string outputFile)
    {
        Console.WriteLine("\n=== Loading validation data for accuracy test ===\n");

        var validationData = File.ReadLines(outputFile)
            .Select(line => JsonSerializer.Deserialize<TrainingEntry>(line) ?? throw new InvalidOperationException($"Failed to deserialize TrainingEntry:\n{line}"))
            .Skip(5000) // Use data not in training
            .Take(100)
            .ToList()!;

        if (validationData.Count > 0)
        {
            ShowNamePredictor.EvaluateAccuracy(validationData);
        }
    }
}
