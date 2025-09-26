using System.IO.Abstractions;
using System.Text.RegularExpressions;

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

    /// <summary>
    /// Generates a relative file path based on the provided pattern string.
    /// Supports placeholders: {TvShowName}, {Season}, {Episode}, {Title}, {Year}
    /// </summary>
    /// <param name="pattern">The pattern string with placeholders to replace</param>
    /// <returns>The formatted relative path</returns>
    /// <exception cref="ArgumentNullException">Thrown when pattern is null</exception>
    /// <exception cref="ArgumentException">Thrown when pattern is empty or whitespace</exception>
    /// <exception cref="InvalidOperationException">Thrown when the episode is not in a valid state</exception>
    internal string GenerateRelativePath(string pattern)
    {
        if (pattern is null)
            throw new ArgumentNullException(nameof(pattern));
            
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern cannot be empty or whitespace.", nameof(pattern));
            
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

        var result = pattern;
        foreach (var replacement in replacements)
        {
            result = result.Replace(replacement.Key, replacement.Value);
        }

        // Clean up any double slashes that might have been created
        result = result.Replace("//", "/");
        
        // Clean up patterns where Year was empty - remove empty parentheses
        result = Regex.Replace(result, @"\s*\(\s*\)", "");
        
        // Clean up extra spaces
        result = Regex.Replace(result, @"\s+", " ").Trim();

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