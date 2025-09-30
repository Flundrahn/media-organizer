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
        "TvShowName", "Season", "Episode", "Title", "Year"
    };

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

    public bool IsOrganized(MediaOrganizerSettings settings)
    {
        if (!IsValid
            || string.IsNullOrWhiteSpace(settings.TvShowDestinationDirectory)
            || string.IsNullOrWhiteSpace(settings.TvShowPathTemplate))
            return false;

        try
        {
            var organizedFullPath = Path.GetFullPath(Path.Combine(settings.TvShowDestinationDirectory, GenerateRelativePath(settings)));
            var currentFullPath = Path.GetFullPath(CurrentFile.FullName); // Helps normalize for path comparison

            return string.Equals(currentFullPath, organizedFullPath, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    // TODO: possibly change this to just generate the full path immediately, since have settings here anyway
    // could reason is out of scope for model, and should just take template as before, 
    // tbh both ways are probably okay 
    // 
    public string GenerateRelativePath(MediaOrganizerSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.TvShowPathTemplate))
            throw new ArgumentException("TvShowPathTemplate cannot be empty or whitespace.", nameof(settings));
            
        return GenerateRelativePathInternal(settings.TvShowPathTemplate);
    }

    private string GenerateRelativePathInternal(string template)
    {
        if (template is null)
            throw new ArgumentNullException(nameof(template));
            
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Template cannot be empty or whitespace.", nameof(template));
            
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
            { "{Year}", Year?.ToString() ?? string.Empty }
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