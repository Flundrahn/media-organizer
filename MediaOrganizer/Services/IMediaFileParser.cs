using System.IO.Abstractions;
using MediaOrganizer.Models;

namespace MediaOrganizer.Services;

/// <summary>
/// Interface for parsing TV show episode information from filenames
/// </summary>
public interface IMediaFileParser
{
    // TODO: possibly replace the CanParse with a TryParse pattern
    /// <summary>
    /// Determines if the given filename can be parsed as a TV show episode
    /// </summary>
    /// <param name="filename">The filename to check</param>
    /// <returns>True if the filename appears to be a TV show episode</returns>
    bool CanParse(string filename);

    /// <summary>
    /// Parses TV show episode information from a file
    /// </summary>
    /// <param name="fileInfo">The file to parse</param>
    /// <returns>Episode information, or invalid TvEpisode if parsing fails</returns>
    IMediaFile Parse(IFileInfo fileInfo);
}