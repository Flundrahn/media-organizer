using System.IO.Abstractions;
using System.Text.RegularExpressions;
using MediaOrganizer.Configuration;

namespace MediaOrganizer.Models;

/// <summary>
/// Contains information about a TV show episode parsed from a filename
/// </summary>
public class TvShowEpisode : IMediaFile
{
    /// <summary>
    /// Valid placeholders that can be used in path templates for TV show episodes
    /// </summary>
    public static readonly HashSet<string> ValidPlaceholders = new()
    {
        "TvShowName", "Season", "Episode", "Title", "Year", "Quality"
    };

    private string _tvShowName = string.Empty;
    public string TvShowName
    {
        get => _tvShowName;
        internal set => _tvShowName = string.IsNullOrWhiteSpace(value) 
            ? string.Empty 
            : value;
    }

    /// <summary>
    /// The season number (1-based)
    /// </summary>
    public int Season { get; internal set; }

    /// <summary>
    /// The episode number within the season (1-based)
    /// </summary>
    public int Episode { get; internal set; }

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        internal set => _title = string.IsNullOrWhiteSpace(value) 
            ? string.Empty 
            : value;
    }

    private string _quality = string.Empty;
    public string Quality
    {
        get => _quality;
        internal set => _quality = string.IsNullOrWhiteSpace(value) 
            ? string.Empty 
            : value;
    }

    public int? Year { get; internal set; }

    public IFileInfo OriginalFile { get; init; }

    public IFileInfo CurrentFile { get; set; }

    public MediaType Type => MediaType.TvShow;

    /// <summary>
    /// Initializes a new instance of TvShowEpisode with file information
    /// </summary>
    /// <param name="fileInfo">The file information to set as both original and current file</param>
    public TvShowEpisode(IFileInfo fileInfo)
    {
        OriginalFile = fileInfo;
        CurrentFile = fileInfo;
    }

    public bool IsValid => !string.IsNullOrWhiteSpace(TvShowName) && Season > 0 && Episode > 0;

    // note: this is a nice method to have, it just troubles me a little bit that it does encourage generating the path multiple times
    // it might be possible to refactor later to be smarter somehow, cache somehow.
    // to check if already organized, I need to generate the path from the settings and compare to current file path.
    // I could return in a try pattern
    public bool IsOrganized(MediaOrganizerSettings settings)
    {
        if (!IsValid
            || string.IsNullOrWhiteSpace(settings.TvShowDestinationDirectory)
            || string.IsNullOrWhiteSpace(settings.TvShowPathTemplate))
            return false;

        var organizedFullPath = GenerateFullPath(settings);
        var currentFullPath = Path.GetFullPath(CurrentFile.FullName); // Helps normalize for path comparison

        return string.Equals(currentFullPath, organizedFullPath, StringComparison.OrdinalIgnoreCase);
    }

    public string GenerateFullPath(MediaOrganizerSettings settings)
    {
        return Path.GetFullPath(Path.Combine(settings.TvShowDestinationDirectory, GenerateRelativePath(settings)));
    }

    public string GenerateRelativePath(MediaOrganizerSettings settings)
    {
        string template = settings.TvShowPathTemplate;

        if (!IsValid)
            throw new InvalidOperationException("Cannot generate path for an invalid TV show episode.");

        var replacements = new Dictionary<string, string>
        {
            { "{TvShowName}", TvShowName },
            { "{Season}", Season.ToString() },
            { "{Season:D2}", Season.ToString("D2") },
            { "{Episode}", Episode.ToString() },
            { "{Episode:D2}", Episode.ToString("D2") },
            { "{Title}", Title ?? string.Empty },
            { "{Year}", Year?.ToString() ?? string.Empty },
            { "{Quality}", Quality ?? string.Empty }
        };

        var result = template;
        foreach (var replacement in replacements)
        {
            result = result.Replace(replacement.Key, replacement.Value);
        }

        // Clean up any double path separators that might have been created
        var doublePathSep = $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}";
        result = result.Replace(doublePathSep, Path.DirectorySeparatorChar.ToString());

        // Clean up patterns where Year was empty - remove empty parentheses
        result = Regex.Replace(result, @"\s*\(\s*\)", "");

        // Clean up extra spaces
        result = Regex.Replace(result, @"\s+", " ").Trim();

        // Always append the original file extension
        var originalExtension = Path.GetExtension(OriginalFile.Name);
        if (!string.IsNullOrEmpty(originalExtension) && !result.EndsWith(originalExtension, StringComparison.OrdinalIgnoreCase))
        {
            result += originalExtension;
        }

        return result;
    }

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