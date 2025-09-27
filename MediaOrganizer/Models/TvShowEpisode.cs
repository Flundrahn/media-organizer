using System.IO.Abstractions;
using System.Text.RegularExpressions;
using MediaOrganizer.Configuration;

namespace MediaOrganizer.Models;

/// <summary>
/// Contains information about a TV show episode parsed from a filename
/// </summary>
public class TvShowEpisode
{
    /// <summary>
    /// Valid placeholders that can be used in path templates
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

    /// <summary>
    /// Determines if the file is already organized (i.e., in its correct destination location)
    /// </summary>
    /// <param name="settings">The media organizer settings containing destination directory and path templates</param>
    /// <returns>True if the current file is already in the correct organized location</returns>
    public bool IsOrganized(MediaOrganizerSettings settings)
    {
        if (!IsValid
            || settings == null
            || string.IsNullOrWhiteSpace(settings.DestinationDirectory)
            || string.IsNullOrWhiteSpace(settings.TvShowPathTemplate))
            return false;

        try
        {
            var expectedRelativePath = GenerateRelativePath(settings.TvShowPathTemplate);
            var expectedFullPath = Path.Combine(settings.DestinationDirectory, expectedRelativePath);
            return CurrentFile.FullName.Equals(expectedFullPath, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates a relative file path based on the provided template string.
    /// Supports placeholders: {TvShowName}, {Season}, {Episode}, {Title}, {Year}
    /// The original file extension is automatically preserved and appended to the result.
    /// </summary>
    /// <param name="template">The template string with placeholders to replace</param>
    /// <returns>The formatted relative path with the original file extension</returns>
    /// <exception cref="ArgumentNullException">Thrown when template is null</exception>
    /// <exception cref="ArgumentException">Thrown when template is empty or whitespace</exception>
    /// <exception cref="InvalidOperationException">Thrown when the episode is not in a valid state</exception>
    internal string GenerateRelativePath(string template)
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