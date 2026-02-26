using System.Text;
using MLModel_TrainingDataApp.Models;

namespace MLModel_TrainingDataApp.Refine;

static class TabularConverter
{
    /// <summary>
    /// Convert refined training entries to TSV format for ML.NET Model Builder.
    /// Output format: filename\tshow_name (tab-separated with header)
    /// </summary>
    public static void ConvertToTsv(List<RefinedTrainingEntry> entries, string outputPath)
    {
        using var writer = new StreamWriter(outputPath, false, Encoding.UTF8);

        // Write header
        writer.WriteLine("filename\tshow_name");

        // Write rows
        foreach (var entry in entries)
        {
            // Escape tabs and newlines in data
            var filename = EscapeField(entry.Filename);
            var showName = EscapeField(entry.ShowName);

            writer.WriteLine($"{filename}\t{showName}");
        }

        Console.WriteLine($"Wrote {entries.Count} entries to {outputPath}");
    }

    private static string EscapeField(string field)
    {
        return field
            .Replace("\t", " ")
            .Replace("\n", " ")
            .Replace("\r", " ")
            .Trim();
    }
}
