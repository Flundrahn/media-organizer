using FFMpegCore;
using MediaOrganizer.Benchmarks.Models;
using System.Diagnostics;
using System.IO.Abstractions;

namespace MediaOrganizer.Benchmarks.Services;

/// <summary>
/// Video metadata extractor using FFMpegCore library
/// Requires FFmpeg/FFprobe binaries to be available in PATH or configured via FFOptions
/// </summary>
public class FFMpegVideoMetadataExtractor 
{
    public bool IsAvailable()
    {
        try
        {
            // Try to run ffprobe with version command to test availability
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe",
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            process.WaitForExit(5000); // 5 second timeout
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<VideoMetadata> ExtractMetadataAsync(IFileInfo videoFile)
    {
        var mediaInfo = await FFProbe.AnalyseAsync(videoFile.FullName);
        
        var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
        var audioStream = mediaInfo.AudioStreams.FirstOrDefault();

        return new VideoMetadata
        {
            File = videoFile,
            Width = videoStream?.Width ?? 0,
            Height = videoStream?.Height ?? 0,
            Duration = mediaInfo.Duration,
            FrameRate = videoStream?.FrameRate ?? 0,
            VideoCodec = videoStream?.CodecName ?? string.Empty,
            AudioCodec = audioStream?.CodecName ?? string.Empty,
            VideoBitrate = videoStream?.BitRate ?? 0,
            AudioBitrate = audioStream?.BitRate ?? 0,
            FileSize = videoFile.Length,
            ContainerFormat = mediaInfo.Format.FormatName ?? string.Empty
        };
    }

    /// <summary>
    /// Extracts only height metadata using minimal FFProbe analysis
    /// Optimized for scenarios where only video height/resolution is needed
    /// </summary>
    public async Task<int> ExtractHeightOnlyAsync(IFileInfo videoFile)
    {
        // Use FFProbe with minimal analysis - only get video stream dimensions
        var mediaInfo = await FFProbe.AnalyseAsync(videoFile.FullName);

        // Return height only if file length is greater than zero
        if (videoFile.Length <= 0)
        {
            return 0;
        }

        return mediaInfo.VideoStreams[0].Height;
    }
}