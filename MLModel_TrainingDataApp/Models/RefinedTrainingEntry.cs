using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MLModel_TrainingDataApp.Models;

/// <summary>
/// Refined training entry with cleaned and normalized show names.
/// Automatically cleans problematic characters during construction.
/// This is the single source of truth for show name cleaning logic.
/// </summary>
public record RefinedTrainingEntry
{
    [JsonPropertyName("filename")]
    public string Filename { get; init; } = string.Empty;

    [JsonPropertyName("show_name")]
    public string ShowName { get; init; } = string.Empty;

    [JsonPropertyName("episode_name")]
    public string EpisodeName { get; init; } = string.Empty;

    [JsonPropertyName("season_number")]
    public int SeasonNumber { get; init; }

    [JsonPropertyName("episode_number")]
    public int EpisodeNumber { get; init; }

    [JsonPropertyName("year")]
    public int? Year { get; init; }

    /// <summary>
    /// Create a refined entry from a raw training entry.
    /// Automatically cleans the show name by removing problematic characters.
    /// </summary>
    public static RefinedTrainingEntry FromRaw(TrainingEntry raw)
    {
        return new RefinedTrainingEntry
        {
            Filename = raw.Filename,
            ShowName = CleanShowName(raw.ShowName),
            EpisodeName = raw.EpisodeName,
            SeasonNumber = raw.SeasonNumber,
            EpisodeNumber = raw.EpisodeNumber,
            Year = raw.Year
        };
    }

    /// <summary>
    /// Clean a show name by removing/replacing problematic characters. We want to create the show name as it would appear in a filename.
    /// This is the single source of truth for all show name transformations.
    /// 
    /// Examples:
    ///   "Star Trek: Voyager" ? "Star Trek Voyager"
    ///   "Nip/Tuck" ? "Nip Tuck"
    ///   "Law & Order" ? "Law And Order"
    ///   "Chip 'n Dale's Rescue Rangers" ? "Chip n Dales Rescue Rangers"
    ///   "My Next Life as a Villainess All Routes Lead to Doom!" ? "My Next Life as a Villainess All Routes Lead to Doom"
    /// </summary>
    private static string CleanShowName(string showName)
    {
        if (string.IsNullOrWhiteSpace(showName))
        {
            return string.Empty;
        }

        var cleaned = showName;
        
        // Handle unicode apostrophes first by converting to regular apostrophe
        cleaned = cleaned.Replace("'", "'");
        
        // Replace problematic punctuation with space or appropriate substitutes
        cleaned = cleaned
            .Replace(":", " ")      // colons
            .Replace("/", " ")      // slashes
            .Replace("&", "And")    // ampersands become "And"
            .Replace("-", " ")      // hyphens
            .Replace("_", " ")      // underscores
            .Replace(".", " ")      // periods
            .Replace("!", " ")      // exclamation marks
            .Replace("?", " ")      // question marks
            .Replace("'", "");      // apostrophes (including possessives)

        // Collapse multiple spaces to single space and trim
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        return cleaned;
    }

    /// <summary>
    /// Get show name tokens for matching.
    /// Splits the cleaned show name into individual words for validation.
    /// </summary>
    public string[] GetShowNameTokens()
    {
        return ShowName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}
