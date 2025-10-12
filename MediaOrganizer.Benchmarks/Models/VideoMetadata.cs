using System.IO.Abstractions;

namespace MediaOrganizer.Benchmarks.Models;

/// <summary>
/// Contains technical metadata extracted from video files
/// </summary>
public class VideoMetadata
{
    /// <summary>
    /// The original file this metadata was extracted from
    /// </summary>
    public IFileInfo File { get; init; } = null!;

    /// <summary>
    /// Video width in pixels
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// Video height in pixels  
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    /// Video resolution as formatted string (e.g., "1920x1080", "1280x720")
    /// </summary>
    public string Resolution => $"{Width}x{Height}";

    /// <summary>
    /// Standard resolution name (e.g., "1080p", "720p", "4K")
    /// </summary>
    public string QualityLabel => GetQualityLabel(Width, Height);

    /// <summary>
    /// Video duration
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Video frame rate (frames per second)
    /// </summary>
    public double FrameRate { get; init; }

    /// <summary>
    /// Video codec (e.g., "H.264", "H.265", "VP9")
    /// </summary>
    public string VideoCodec { get; init; } = string.Empty;

    /// <summary>
    /// Audio codec (e.g., "AAC", "AC3", "DTS")
    /// </summary>
    public string AudioCodec { get; init; } = string.Empty;

    /// <summary>
    /// Video bitrate in bits per second
    /// </summary>
    public long VideoBitrate { get; init; }

    /// <summary>
    /// Audio bitrate in bits per second
    /// </summary>
    public long AudioBitrate { get; init; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; init; }

    /// <summary>
    /// Container format (e.g., "MP4", "MKV", "AVI")
    /// </summary>
    public string ContainerFormat { get; init; } = string.Empty;

    /// <summary>
    /// Whether the video metadata was successfully extracted
    /// </summary>
    public bool IsValid => Width > 0 && Height > 0 && Duration > TimeSpan.Zero;

    private static string GetQualityLabel(int width, int height)
    {
        return height switch
        {
            >= 2160 => "4K",
            >= 1440 => "1440p", 
            >= 1080 => "1080p",
            >= 720 => "720p",
            >= 480 => "480p",
            >= 360 => "360p",
            _ => $"{height}p"
        };
    }

    /// <summary>
    /// Compares the extracted metadata quality with filename-based quality
    /// </summary>
    /// <param name="filenameQuality">Quality extracted from filename</param>
    /// <returns>True if metadata quality matches filename quality</returns>
    public bool MatchesFilenameQuality(string filenameQuality)
    {
        if (string.IsNullOrEmpty(filenameQuality))
            return false;

        var normalizedFilename = filenameQuality.ToUpperInvariant();
        var normalizedMetadata = QualityLabel.ToUpperInvariant();

        return normalizedFilename == normalizedMetadata ||
               (normalizedFilename == "4K" && normalizedMetadata == "2160P") ||
               (normalizedFilename == "2160P" && normalizedMetadata == "4K");
    }
}