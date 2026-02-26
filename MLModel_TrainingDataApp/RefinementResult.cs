using MLModel_TrainingDataApp.Models;

namespace MLModel_TrainingDataApp;

public record RefinementResult
{
    public required List<RefinedTrainingEntry> Balanced { get; init; }
    public required string TsvPath { get; init; }
    public required string NerPath { get; init; }
    public required int CleanedCount { get; init; }
    public required int DuplicatesRemoved { get; init; }
    public required int InvalidRemoved { get; init; }
    public required int ExcessRemoved { get; init; }
}
