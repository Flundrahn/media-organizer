using System.IO.Abstractions;

namespace MediaOrganizer.Models;

/// <summary>
/// Contains information about a TV show episode parsed from a filename
/// </summary>
public class TvShowEpisode
{
    public string TvShowName { get; internal set; } = string.Empty;

    /// <summary>
    /// The season number (1-based)
    /// </summary>
    public int Season { get; internal set; }

    /// <summary>
    /// The episode number within the season (1-based)
    /// </summary>
    public int Episode { get; internal set; }

    public string Title { get; internal set; } = string.Empty;

    public int? Year { get; internal set; }

    /// <summary>
    /// The original file info when the episode was first parsed
    /// </summary>
    public IFileInfo OriginalFile { get; init; }

    /// <summary>
    /// The current file info, which may be different from original if the file has been moved
    /// </summary>
    public IFileInfo CurrentFile { get; internal set; }

    /// <summary>
    /// Initializes a new instance of TvShowEpisode with file information
    /// </summary>
    /// <param name="fileInfo">The file information to set as both original and current file</param>
    public TvShowEpisode(IFileInfo fileInfo)
    {
        OriginalFile = fileInfo;
        CurrentFile = fileInfo;
    }

    /// <summary>
    /// Whether the parsing was successful
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(TvShowName) && Season > 0 && Episode > 0;

    public override string ToString()
    {
        var result = $"{TvShowName} S{Season:D2}E{Episode:D2}";
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