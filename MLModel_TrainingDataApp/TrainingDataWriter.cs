using System.Text.Json;
using MLModel_TrainingDataApp.Models;
using MLModel_TrainingDataApp.OpenSubtitles;

namespace MLModel_TrainingDataApp;

public class TrainingDataWriter : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };
    private readonly StreamWriter _writer;

    public int Total { get; private set; }

    private TrainingDataWriter(StreamWriter writer, int existingCount)
    {
        _writer = writer;
        Total = existingCount;
    }

    public static async Task<TrainingDataWriter> CreateAsync(string outputFile)
    {
        int existingCount = File.Exists(outputFile)
            ? File.ReadLines(outputFile).Count(l => !string.IsNullOrWhiteSpace(l))
            : 0;

        if (existingCount > 0)
            Console.WriteLine($"Resuming — {existingCount} entries already written.");

        var streamWriter = new StreamWriter(outputFile, append: true);
        await streamWriter.FlushAsync();
        return new TrainingDataWriter(streamWriter, existingCount);
    }

    public async Task WriteEntryAsync(OpenSubtitlesSubtitleItem item)
    {
        if (item == null)
        {
            return;
        }

        var attrs = item.Attributes;
        var fd = attrs?.FeatureDetails;
        if (fd == null ||
            string.IsNullOrWhiteSpace(attrs?.Release) ||
            string.IsNullOrWhiteSpace(fd.ParentTitle) ||
            fd.SeasonNumber is null ||
            fd.SeasonNumber == 0 ||
            fd.EpisodeNumber is null ||
            fd.EpisodeNumber == 0)
        {
            return;
        }

        var entry = new TrainingEntry(
            Filename: attrs.Release,
            ShowName: fd.ParentTitle,
            EpisodeName: fd.EpisodeTitle ?? "",
            SeasonNumber: fd.SeasonNumber.Value,
            EpisodeNumber: fd.EpisodeNumber.Value,
            Year: fd.Year
        );

        await _writer.WriteLineAsync(JsonSerializer.Serialize(entry, JsonOptions));
        Total += 1;
        return;
    }

    public async ValueTask DisposeAsync()
    {
        await _writer.FlushAsync();
        await _writer.DisposeAsync();
        Console.WriteLine($"Done. {Total} total entries in file.");
    }
}
