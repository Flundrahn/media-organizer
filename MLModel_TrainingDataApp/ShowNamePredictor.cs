using System.Text.RegularExpressions;
using MLModel_TrainingDataApp.Models;
using MLModel1_ConsoleApp2;

namespace MLModel_TrainingDataApp;

/// <summary>
/// Helper class to test the trained NER model and extract show names from filenames.
/// </summary>
static class ShowNamePredictor
{
    /// <summary>
    /// Predict show name from a filename using the trained NER model.
    /// </summary>
    public static string? PredictShowName(string filename)
    {
        // Tokenize the filename (same way as training data)
        var tokens = Tokenize(filename);
        
        if (tokens.Count == 0)
            return null;
        
        // Create model input - ML.NET expects sentence as space-separated tokens
        var input = new MLModel1.ModelInput
        {
            Sentence = string.Join(" ", tokens)
        };
        
        // Get prediction
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
                // Stop at first O after finding show tokens
                break;
            }
        }
        
        return showTokens.Count > 0 ? string.Join(" ", showTokens) : null;
    }
    
    /// <summary>
    /// Test the model on multiple filenames and display results.
    /// </summary>
    public static void TestOnExamples(string[] testFilenames)
    {
        Console.WriteLine("=== Testing NER Model on Sample Filenames ===\n");
        Console.WriteLine($"{"Filename",-60} | Predicted Show Name");
        Console.WriteLine(new string('-', 100));
        
        foreach (var filename in testFilenames)
        {
            var predicted = PredictShowName(filename);
            Console.WriteLine($"{filename,-60} | {predicted ?? "(none)"}");
        }
    }
    
    /// <summary>
    /// Evaluate model accuracy on validation data.
    /// </summary>
    public static void EvaluateAccuracy(List<TrainingEntry> validationData)
    {
        Console.WriteLine("\n=== Evaluating Model Accuracy ===\n");
        
        int correct = 0;
        int total = validationData.Count;
        var errors = new List<(string Filename, string Expected, string? Predicted)>();
        
        foreach (var entry in validationData.Take(100)) // Test on first 100
        {
            var predicted = PredictShowName(entry.Filename);
            var expected = entry.ShowName;
            
            // Normalize for comparison (case-insensitive, trim)
            var normalizedPredicted = predicted?.Trim().ToLowerInvariant();
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
        
        double accuracy = 100.0 * correct / Math.Min(100, total);
        Console.WriteLine($"Accuracy: {correct}/{Math.Min(100, total)} = {accuracy:F1}%\n");
        
        if (errors.Count > 0)
        {
            Console.WriteLine($"Sample Errors (showing first 10 of {errors.Count}):");
            Console.WriteLine($"{"Filename",-50} | {"Expected",-25} | Predicted");
            Console.WriteLine(new string('-', 110));
            
            foreach (var (filename, expected, predicted) in errors.Take(10))
            {
                Console.WriteLine($"{filename,-50} | {expected,-25} | {predicted ?? "(none)"}");
            }
        }
    }
    
    private static List<string> Tokenize(string filename)
    {
        // Split on dots, dashes, underscores, spaces (same as training)
        return Regex.Split(filename, @"[\.\-_\s]+")
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();
    }
}
