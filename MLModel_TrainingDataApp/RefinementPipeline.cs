using MLModel_TrainingDataApp.Models;
using MLModel_TrainingDataApp.Refine;

namespace MLModel_TrainingDataApp;

public class RefinementPipeline
{
    private readonly ITrainingDataFileWriter _writer;

    public RefinementPipeline(ITrainingDataFileWriter writer)
    {
        _writer = writer;
    }

    public RefinementResult Process(List<TrainingEntry> raw)
    {
        Console.WriteLine("Phase 1: Cleaning show names...");
        var cleaned = raw.Select(RefinedTrainingEntry.FromRaw).ToList();
        Console.WriteLine($"  ? Cleaned {cleaned.Count} entries (removed colons, slashes, etc.)\n");

        var (unique, duplicates) = Deduplicator.Deduplicate(cleaned);
        Console.WriteLine();
        _writer.SaveInspectionFile("refined_removed_duplicates.jsonl", duplicates);

        var (valid, invalid) = TokenPresenceValidator.ValidateEntries(unique);
        Console.WriteLine();
        _writer.SaveInspectionFile("refined_removed_invalid_show_names.jsonl", invalid);

        var (balanced, removed) = ShowBalancer.BalanceByShow(valid, maxPerShow: 50);
        Console.WriteLine();
        _writer.SaveInspectionFile("refined_removed_excess_per_show.jsonl", removed);

        var tsvPath = _writer.WriteTrainingDataTsv(balanced);

        Console.WriteLine();
        var nerPath = _writer.WriteTrainingDataNer(balanced);

        return new RefinementResult
        {
            Balanced = balanced,
            TsvPath = tsvPath,
            NerPath = nerPath,
            CleanedCount = cleaned.Count,
            DuplicatesRemoved = duplicates.Count,
            InvalidRemoved = invalid.Count,
            ExcessRemoved = removed.Count
        };
    }
}
