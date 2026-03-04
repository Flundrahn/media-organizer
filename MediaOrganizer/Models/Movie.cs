using MediaOrganizer.Configuration;
using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace MediaOrganizer.Models;

/// <summary>
/// Contains information about a movie parsed from a filename
/// </summary>
public class Movie : IMediaFile
{
    /// <summary>
    /// Valid placeholders that can be used in path templates for movies
    /// </summary>
    public static readonly HashSet<string> ValidPlaceholders = new()
    {
        "Title", "Year", "Quality"
    };

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        internal set
        {
            _title = string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value;
        }
    }

    public int? Year { get; internal set; }

    private string _quality = string.Empty;
    public string Quality
    {
        get => _quality;
        internal set
        {
            _quality = string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value;
        }
    }

    public string OriginalFilePath { get; init; }

    public string CurrentFilePath { get; set; } 

    public MediaType Type => MediaType.Movie;

    public Movie(IFileInfo fileInfo)
    {
        OriginalFilePath = fileInfo.FullName;
        CurrentFilePath = fileInfo.FullName;
        //TODO: save also relative path
    }

    public bool IsValid => !string.IsNullOrWhiteSpace(Title);

    public void SetCurrentFilePath(IFileInfo fileInfo)
    {
        CurrentFilePath = fileInfo.FullName;
    }

    public bool IsOrganized(MediaOrganizerSettings settings)
    {
        if (!IsValid)
        {
            return false;
        }

        var organizedFullPath = GenerateFullPath(settings);
        var currentFullPath = Path.GetFullPath(CurrentFilePath); // Helps normalize for path comparison

        return string.Equals(currentFullPath, organizedFullPath, StringComparison.OrdinalIgnoreCase);
    }

    public string GenerateFullPath(MediaOrganizerSettings settings)
    {
        return Path.GetFullPath(Path.Combine(settings.MovieDestinationDirectory, GenerateRelativePath(settings)));
    }

    public string GenerateRelativePath(MediaOrganizerSettings settings)
    {
        string template = settings.MoviePathTemplate;

        if (!IsValid)
            throw new InvalidOperationException("Cannot generate path for an invalid movie.");

        var replacements = new Dictionary<string, string>
        {
            { "{Title}", Title },
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

        // Clean up patterns where Quality was empty - remove empty brackets
        result = Regex.Replace(result, @"\s*\[\s*\]", "");

        // Clean up extra spaces
        result = Regex.Replace(result, @"\s+", " ").Trim();

        // NOTE: Here should consider use extension of current file later, when introduce DB, leave for now
        // Always append the original file extension

        var originalExtension = Path.GetExtension(OriginalFilePath);
        if (!string.IsNullOrEmpty(originalExtension) && !result.EndsWith(originalExtension, StringComparison.OrdinalIgnoreCase))
        {
            result += originalExtension;
        }

        return result;
    }

    public override string ToString()
    {
        var result = Title;
        if (Year is not null)
        {
            result += $" ({Year})";
        }
        if (!string.IsNullOrWhiteSpace(Quality))
        {
            result += $" [{Quality}]";
        }
        return result;
    }
}
