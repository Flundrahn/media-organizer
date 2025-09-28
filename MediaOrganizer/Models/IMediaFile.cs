using System.IO.Abstractions;
using MediaOrganizer.Configuration;

namespace MediaOrganizer.Models;

/// <summary>
/// Represents a media file that can be organized by the MediaFileOrganizer.
/// This interface provides a common abstraction for different types of media files (TV shows, movies, etc.).
/// </summary>
public interface IMediaFile
{
    /// <summary>
    /// The original file info when the media file was first parsed
    /// </summary>
    IFileInfo OriginalFile { get; }

    /// <summary>
    /// The current file info, which may be different from original if the file has been moved
    /// </summary>
    IFileInfo CurrentFile { get; set; }

    /// <summary>
    /// Whether the parsing was successful and the media file contains valid information
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// The type of media file (TV show, movie, etc.)
    /// </summary>
    MediaType Type { get; }

    /// <summary>
    /// Generates a relative file path based on the provided template string.
    /// The original file extension is automatically preserved and appended to the result.
    /// </summary>
    /// <param name="template">The template string with placeholders to replace</param>
    /// <returns>The formatted relative path with the original file extension</returns>
    /// <exception cref="ArgumentNullException">Thrown when template is null</exception>
    /// <exception cref="ArgumentException">Thrown when template is empty or whitespace</exception>
    /// <exception cref="InvalidOperationException">Thrown when the media file is not in a valid state</exception>
    string GenerateRelativePath(string template);

    /// <summary>
    /// Determines if the file is already organized (i.e., in its correct destination location)
    /// </summary>
    /// <param name="settings">The media organizer settings containing destination directory and path templates</param>
    /// <returns>True if the current file is already in the correct organized location</returns>
    bool IsOrganized(MediaOrganizerSettings settings);
}

/// <summary>
/// Enumeration of supported media file types
/// </summary>
public enum MediaType
{
    TvShow,
    Movie
}