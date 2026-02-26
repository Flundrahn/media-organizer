using MLModel_TrainingDataApp.Models;

namespace MLModel_TrainingDataApp.Refine;

static class ShowBalancer
{
    public static (List<RefinedTrainingEntry> Balanced, List<RefinedTrainingEntry> Removed) BalanceByShow(List<RefinedTrainingEntry> entries, int maxPerShow = 50)
    {
        var balanced = new List<RefinedTrainingEntry>();
        var removed = new List<RefinedTrainingEntry>();
        
        foreach (var group in entries.GroupBy(e => e.ShowName))
        {
            var kept = group.Take(maxPerShow).ToList();
            var excess = group.Skip(maxPerShow).ToList();
            
            balanced.AddRange(kept);
            removed.AddRange(excess);
        }
        
        var showCounts = entries
            .GroupBy(e => e.ShowName)
            .Select(g => g.Count())
            .ToList();
        
        if (showCounts.Count > 0)
        {
            var avgBefore = showCounts.Average();
            var maxBefore = showCounts.Max();
            Console.WriteLine($"Show balancing: capped at {maxPerShow} per show (was avg={avgBefore:F1}, max={maxBefore})");
        }
        else
        {
            Console.WriteLine($"Show balancing: no entries to balance");
        }
        
        return (balanced, removed);
    }
}
