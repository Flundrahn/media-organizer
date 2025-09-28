using System.IO.Abstractions;
using System.Text.RegularExpressions;
using MediaOrganizer.Configuration;

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

    public string Title { get; internal set; } = string.Empty;

    public int? Year { get; internal set; }

    public string Quality { get; internal set; } = string.Empty;

    public IFileInfo OriginalFile { get; init; }

    public IFileInfo CurrentFile { get; set; }

    public MediaType Type => MediaType.Movie;

    /// <summary>
    /// Initializes a new instance of Movie with file information
    /// </summary>
    /// <param name="fileInfo">The file information to set as both original and current file</param>
    public Movie(IFileInfo fileInfo)
    {
        OriginalFile = fileInfo;
        CurrentFile = fileInfo;
    }

    public bool IsValid => !string.IsNullOrWhiteSpace(Title) && Year.HasValue && Year > 0;

    public bool IsOrganized(MediaOrganizerSettings settings)
    {
        if (!IsValid
            || string.IsNullOrWhiteSpace(settings.DestinationDirectory)
            || string.IsNullOrWhiteSpace(settings.MoviePathTemplate))
            return false;

        try
        {
            var organizedFullPath = Path.GetFullPath(Path.Combine(settings.DestinationDirectory, GenerateRelativePath(settings)));
            var currentFullPath = Path.GetFullPath(CurrentFile.FullName);

            return string.Equals(currentFullPath, organizedFullPath, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public string GenerateRelativePath(MediaOrganizerSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.MoviePathTemplate))
            throw new ArgumentException("MoviePathTemplate cannot be empty or whitespace.", nameof(settings));
            
        return GenerateRelativePathInternal(settings.MoviePathTemplate);
    }

    private string GenerateRelativePathInternal(string template)
    {
        if (template is null)
            throw new ArgumentNullException(nameof(template));
            
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Template cannot be empty or whitespace.", nameof(template));
            
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
        
        // Clean up patterns where Quality was empty - remove empty brackets
        result = Regex.Replace(result, @"\s*\[\s*\]", "");
        
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
        var result = $"{Title} ({Year})";
        if (!string.IsNullOrWhiteSpace(Quality))
        {
            result += $" [{Quality}]";
        }
        return result;
    }
}