// ML.NET NER Model Testing Program
// Tests the retrained Named Entity Recognition model for show name extraction

using System.Text.Json;
using System.Text.RegularExpressions;
using MLModel1_ConsoleApp2;

// Sample test filenames covering various formats
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
    "Dawsons.Creek.S04E09.720p",
    "The.Big.Valley.S01E21.Barbary.Red.1080p",
    "The.Twilight.Zone.1985.S01E12.Her.Pilgrim.Soul",
    "Arcane.S01E09.The.Monster.You.Created.MN"
};

// Display menu and handle modes
Console.WriteLine("╔════════════════════════════════════════════════════════╗");
Console.WriteLine("║   ML.NET NER Model - Show Name Extraction Testing      ║");
Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");

Console.WriteLine("Available modes:");
Console.WriteLine("  1 - Test on sample filenames (batch mode)");
Console.WriteLine("  2 - Interactive mode (test single filenames)");
Console.WriteLine("  3 - Accuracy evaluation on validation data");
Console.WriteLine("  4 - Test all modes\n");

Console.Write("Select mode (1-4): ");
var mode = Console.ReadLine()?.Trim() ?? "1";

switch (mode)
{
    case "1":
        TestBatch(testFilenames);
        break;
    case "2":
        InteractiveMode();
        break;
    case "3":
        EvaluateAccuracy();
        break;
    case "4":
        TestBatch(testFilenames);
        Console.WriteLine("\n" + new string('─', 100) + "\n");
        EvaluateAccuracy();
        break;
    default:
        Console.WriteLine("Invalid mode. Running batch test...");
        TestBatch(testFilenames);
        break;
}

Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
Console.WriteLine("║                 Testing Complete                       ║");
Console.WriteLine("╚════════════════════════════════════════════════════════╝");

// ───────────────────────────────────────────────────────────────────────────
// Mode 1: Batch Testing
// ───────────────────────────────────────────────────────────────────────────

void TestBatch(string[] filenames)
{
    Console.WriteLine("\n=== Testing NER Model on Sample Filenames ===\n");
    Console.WriteLine($"{"Filename",-65} | {"Predicted Show Name",-35}");
    Console.WriteLine(new string('─', 105));

    int successCount = 0;
    int failureCount = 0;

    foreach (var filename in filenames)
    {
        var predicted = PredictShowName(filename);
        
        if (predicted != null)
            successCount++;
        else
            failureCount++;

        var displayPrediction = predicted ?? "(none)";
        Console.WriteLine($"{filename,-65} | {displayPrediction,-35}");
    }

    Console.WriteLine(new string('─', 105));
    Console.WriteLine($"\nResults: {successCount} successful, {failureCount} no match detected\n");
}

// ───────────────────────────────────────────────────────────────────────────
// Mode 2: Interactive Testing
// ───────────────────────────────────────────────────────────────────────────

void InteractiveMode()
{
    Console.WriteLine("\n=== Interactive Show Name Prediction ===\n");
    Console.WriteLine("Enter filenames to test (type 'exit' or 'quit' to end):\n");

    while (true)
    {
        Console.Write("> ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(input) || 
            input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("quit", StringComparison.OrdinalIgnoreCase))
            break;

        var predicted = PredictShowName(input);
        Console.WriteLine($"  Predicted: {predicted ?? "(none)"}\n");
    }
}

// ───────────────────────────────────────────────────────────────────────────
// Mode 3: Accuracy Evaluation
// ───────────────────────────────────────────────────────────────────────────

void EvaluateAccuracy()
{
    const string trainingDataPath = @"C:\Users\fredr\source\repos\media-organizer\TempConsole\Data\tv_episode_training_data.jsonl";

    if (!File.Exists(trainingDataPath))
    {
        Console.WriteLine($"\n⚠ Training data file not found: {trainingDataPath}");
        return;
    }

    Console.WriteLine("\n=== Evaluating Model Accuracy on Validation Data ===\n");
    Console.WriteLine("Loading training data...");

    // Load training entries
    var trainingData = File.ReadLines(trainingDataPath)
        .Select(line => JsonSerializer.Deserialize<TrainingEntry>(line))
        .Where(e => e != null)
        .ToList()!;

    if (trainingData.Count == 0)
    {
        Console.WriteLine("No training data found.");
        return;
    }

    // Use data after first 5000 entries for validation (unseen data)
    var validationData = trainingData
        .Skip(5000)
        .Take(200)
        .ToList();

    if (validationData.Count == 0)
    {
        Console.WriteLine("Not enough training data for validation set.");
        return;
    }

    Console.WriteLine($"Testing on {validationData.Count} validation entries...\n");

    int correct = 0;
    var errors = new List<(string Filename, string Expected, string? Predicted)>();

    foreach (var entry in validationData)
    {
        var predicted = PredictShowName(entry.Filename);
        var expected = entry.ShowName;

        // Normalize for comparison (case-insensitive, trim)
        var normalizedPredicted = predicted?.Trim().ToLowerInvariant() ?? "";
        var normalizedExpected = expected.Trim().ToLowerInvariant();

        if (normalizedPredicted == normalizedExpected)
        {
            correct++;
        }
        else
        {
            errors.Add((entry.Filename, expected, predicted));
        }
    }

    double accuracy = 100.0 * correct / validationData.Count;
    
    Console.WriteLine("┌─────────────────────────────────────────────┐");
    Console.WriteLine($"│ Accuracy: {correct}/{validationData.Count} = {accuracy:F1}%{new string(' ', accuracy.ToString("F1").Length > 3 ? 22 : 24)}│");
    Console.WriteLine("└─────────────────────────────────────────────┘\n");

    if (errors.Count > 0)
    {
        int displayCount = Math.Min(15, errors.Count);
        Console.WriteLine($"Sample Errors (showing {displayCount} of {errors.Count}):\n");
        Console.WriteLine($"{"Filename",-50} | {"Expected",-25} | {"Predicted",-25}");
        Console.WriteLine(new string('─', 105));

        foreach (var (filename, expected, predicted) in errors.Take(displayCount))
        {
            Console.WriteLine($"{filename,-50} | {expected,-25} | {predicted ?? "(none)",-25}");
        }
    }
    else
    {
        Console.WriteLine("✓ Perfect accuracy! No errors found.\n");
    }
}

// ───────────────────────────────────────────────────────────────────────────
// Helper: Predict show name from filename
// ───────────────────────────────────────────────────────────────────────────

string? PredictShowName(string filename)
{
    var tokens = Tokenize(filename);

    if (tokens.Count == 0)
        return null;

    // Create model input
    var input = new MLModel1.ModelInput
    {
        Sentence = string.Join(" ", tokens)
    };

    // Get prediction from NER model
    var output = MLModel1.Predict(input);

    // Extract tokens labeled as B-SHOW or I-SHOW
    var showTokens = new List<string>();

    for (int i = 0; i < tokens.Count && i < output.Predicted_label.Length; i++)
    {
        var label = output.Predicted_label[i];
        if (label == "B-SHOW" || label == "I-SHOW")
        {
            showTokens.Add(tokens[i]);
        }
        else if (showTokens.Count > 0)
        {
            // Stop at first non-SHOW label after finding show tokens
            break;
        }
    }

    return showTokens.Count > 0 ? string.Join(" ", showTokens) : null;
}

// ───────────────────────────────────────────────────────────────────────────
// Helper: Tokenize filename
// ───────────────────────────────────────────────────────────────────────────

List<string> Tokenize(string filename)
{
    // Split on dots, dashes, underscores, spaces (same as training data)
    return Regex.Split(filename, @"[\.\-_\s]+")
        .Where(t => !string.IsNullOrWhiteSpace(t))
        .ToList();
}

// ───────────────────────────────────────────────────────────────────────────
// DTO: Training Entry (matches TempConsole training data format)
// ───────────────────────────────────────────────────────────────────────────

record TrainingEntry(
    string Filename,
    string ShowName,
    string EpisodeName,
    int SeasonNumber,
    int EpisodeNumber,
    int? Year
);

