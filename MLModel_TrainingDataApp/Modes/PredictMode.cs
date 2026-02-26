using MLModel_TrainingDataApp;

namespace MLModel_TrainingDataApp.Modes;

/// <summary>
/// Predict mode: interactive show name prediction from filenames.
/// </summary>
public static class PredictMode
{
    public static void Execute(string[] args)
    {
        Console.WriteLine("=== Interactive Show Name Prediction ===\n");

        if (args.Length > 1)
        {
            ExecuteSinglePrediction(args[1]);
        }
        else
        {
            ExecuteInteractiveLoop();
        }
    }

    private static void ExecuteSinglePrediction(string filename)
    {
        var predicted = ShowNamePredictor.PredictShowName(filename);
        Console.WriteLine($"Filename: {filename}");
        Console.WriteLine($"Predicted Show: {predicted ?? "(none)"}");
    }

    private static void ExecuteInteractiveLoop()
    {
        Console.WriteLine("Enter filenames to test (or 'quit' to exit):\n");

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                break;

            var predicted = ShowNamePredictor.PredictShowName(input);
            Console.WriteLine($"  Predicted: {predicted ?? "(none)"}\n");
        }
    }
}
