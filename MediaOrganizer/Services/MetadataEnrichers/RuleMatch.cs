namespace MediaOrganizer.Services.MetadataEnrichers;

/// <summary>
/// Result from applying a single extraction rule.
/// </summary>
public class RuleMatch
{
    public string? ShowName { get; init; }
    public int? SeasonNumber { get; init; }
    public int? EpisodeNumber { get; init; }
    public string? EpisodeTitle { get; init; }
    public int? Year { get; init; }
    public float Confidence { get; init; }
    public string RuleName { get; init; } = string.Empty;
}
