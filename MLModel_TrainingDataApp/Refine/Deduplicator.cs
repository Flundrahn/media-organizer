using MLModel_TrainingDataApp.Models;

namespace MLModel_TrainingDataApp.Refine;

static class Deduplicator
{
    public static (List<RefinedTrainingEntry> Unique, List<RefinedTrainingEntry> Duplicates) Deduplicate(List<RefinedTrainingEntry> entries)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var unique = new List<RefinedTrainingEntry>();
        var duplicates = new List<RefinedTrainingEntry>();

        foreach (var entry in entries)
        {
            if (seen.Add(entry.Filename))
            {
                unique.Add(entry);
            }
            else
            {
                duplicates.Add(entry);
            }
        }

        Console.WriteLine($"Deduplication: {entries.Count} ? {unique.Count} ({duplicates.Count} duplicates removed)");

        return (unique, duplicates);
    }
}
