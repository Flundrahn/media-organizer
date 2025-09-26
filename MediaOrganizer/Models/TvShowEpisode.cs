namespace MediaOrganizer.Models;

/// <summary>
/// Contains information about a TV show episode parsed from a filename
/// </summary>
public class TvShowEpisode
{
    /// <summary>
    /// The name of the TV show
    /// </summary>
    public string ShowName { get; set; } = string.Empty;

    /// <summary>
    /// The season number (1-based)
    /// </summary>
    public int Season { get; set; }

    /// <summary>
    /// The episode number within the season (1-based)
    /// </summary>
    public int Episode { get; set; }

    /// <summary>
    /// The title of the episode, if available in the filename
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The year of the show, useful for disambiguation
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Whether the parsing was successful
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(ShowName) && Season > 0 && Episode > 0;

    public override string ToString()
    {
        var result = $"{ShowName} S{Season:D2}E{Episode:D2}";
        if (!string.IsNullOrWhiteSpace(Title))
        {
            result += $" - {Title}";
        }
        if (Year.HasValue)
        {
            result += $" ({Year})";
        }
        return result;
    }
}