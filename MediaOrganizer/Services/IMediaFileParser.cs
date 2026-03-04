using System.IO.Abstractions;
using MediaOrganizer.Models;

namespace MediaOrganizer.Services;

public interface IMediaFileParser
{
    // TODO: possibly replace the CanParse with a TryParse pattern
    bool CanParse(string filename);

    /// <summary>
    /// Parses TV show episode information from a file
    /// </summary>
    /// <param name="fileInfo">The file to parse</param>
    /// <returns>Episode information, or invalid TvEpisode if parsing fails</returns>
    IMediaFile Parse(IFileInfo fileInfo);
}
