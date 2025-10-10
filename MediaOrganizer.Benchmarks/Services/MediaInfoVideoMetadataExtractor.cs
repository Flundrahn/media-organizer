using MediaInfoDotNet;
using MediaInfoDotNet.Models;
using MediaOrganizer.Models;
using System.IO.Abstractions;

namespace MediaOrganizer.Benchmarks.Services;

/// <summary>
/// Video metadata extractor using MediaInfo library
/// Requires MediaInfo library to be installed on the system
/// </summary>
public class MediaInfoVideoMetadataExtractor
{
    public bool IsAvailable()
    {
        try
        {
            // Simple test: try to create a MediaInfo instance to verify the native DLL loads
            using var mediaInfo = new MediaInfoLib.MediaInfo();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MediaInfo not available: {ex.Message}");
            return false;
        }
    }

    public Task<VideoMetadata> ExtractMetadataAsync(IFileInfo videoFile)
    {
        var mediaFile = new MediaFile(videoFile.FullName);

        // Extract video stream information - using safe access with null checks
        if (mediaFile.Video.Count == 0)
        {
            return Task.FromResult(new VideoMetadata
            {
                File = videoFile,
                FileSize = videoFile.Length
            });
        }

        VideoStream video = mediaFile.Video[0]; 
        var width = video.Width;
        var height = video.Height;
        var frameRate = video.FrameRate;
        var videoCodec = video.Format;

        // Handle bitrate as string and convert to long
        var videoBitrateStr = video.BitRate;
        var videoBitrate = long.TryParse(videoBitrateStr, out var vBitrate) ? vBitrate : 0;

        // Extract audio stream information
        var audio = mediaFile.Audio.FirstOrDefault();
        var audioCodec = audio?.Format ?? string.Empty;
        var audioBitrateStr = audio?.BitRate;
        var audioBitrate = long.TryParse(audioBitrateStr, out var aBitrate) 
            ? aBitrate 
            : 0;

        // Extract general information
        var durationMs = mediaFile.General?.Duration ?? 0;
        var duration = TimeSpan.FromMilliseconds(durationMs);
        var containerFormat = mediaFile.General?.Format ?? string.Empty;

        return Task.FromResult(new VideoMetadata
        {
            File = videoFile,
            Width = width,
            Height = height,
            Duration = duration,
            FrameRate = frameRate,
            VideoCodec = videoCodec,
            AudioCodec = audioCodec,
            VideoBitrate = videoBitrate,
            AudioBitrate = audioBitrate,
            FileSize = videoFile.Length,
            ContainerFormat = containerFormat
        });
    }

    /// <summary>
    /// Extracts only height metadata using minimal MediaInfo analysis
    /// Optimized for scenarios where only video height is needed
    /// </summary>
    public static Task<int> ExtractHeightOnlyAsync(IFileInfo videoFile)
    {
        var mediaFile = new MediaFile(videoFile.FullName);
        if (mediaFile.Video.Count == 0)
        {
            return Task.FromResult(0);
        }

        return Task.FromResult(mediaFile.Video[0].Height);
    }
}