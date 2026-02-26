using System.Text.Json;
using MLModel_TrainingDataApp.Models;
using MLModel_TrainingDataApp.Common;
using MLModel_TrainingDataApp.OpenSubtitles;

namespace MLModel_TrainingDataApp.Modes;

/// <summary>
/// Fetch mode: fetches TV episode data from OpenSubtitles API for training.
/// Uses tmdb_id directly for efficient subtitle lookups.
/// </summary>
public class FetchMode
{
    private readonly string _checkpointFile;
    private readonly string _showsFile;
    private readonly OpenSubtitlesClient _client;
    private readonly TrainingDataWriter _writer;
    private readonly int _perShowLimit;
    private readonly int _targetCount;
    private readonly int _baseDelayMs;

    public FetchMode(
        string checkpointFile,
        string showsFile,
        OpenSubtitlesClient client,
        TrainingDataWriter writer,
        int perShowLimit,
        int targetCount,
        int baseDelayMs)
    {
        _checkpointFile = checkpointFile;
        _showsFile = showsFile;
        _client = client;
        _writer = writer;
        _perShowLimit = perShowLimit;
        _targetCount = targetCount;
        _baseDelayMs = baseDelayMs;
    }

    public async Task ExecuteAsync()
    {
        if (!File.Exists(_showsFile))
        {
            Console.WriteLine($"Shows file not found: {_showsFile}");
            Console.WriteLine($"Expected location: {_showsFile}");
            Console.WriteLine("\nPlease ensure shows_list.json is in the Data folder.");
            return;
        }

        Console.WriteLine($"Using shows file: {_showsFile}");

        var showsJson = JsonSerializer.Deserialize<ShowEntry[]>(File.ReadAllText(_showsFile))
            ?? Array.Empty<ShowEntry>();
        var checkpoint = await CheckpointStore.LoadAsync(_checkpointFile)
            ?? new Checkpoint { ShowIndex = 0, NextPage = 1, TotalWritten = _writer.Total };

        for (int showIndex = checkpoint.ShowIndex; showIndex < showsJson.Length; showIndex++)
        {
            if (_writer.Total >= _targetCount) break;

            var showElem = showsJson[showIndex];

            Console.WriteLine(
                $"[{_writer.Total}] Fetching up to {_perShowLimit} samples for show index {showIndex}: " +
                $"{showElem.Name} (tmdb_id: {showElem.TmdbId})");

            int startPage = (showIndex == checkpoint.ShowIndex) ? checkpoint.NextPage : 1;
            int collected = await FetchByTmdbIdAsync(showIndex, showElem.TmdbId, showElem.Name, startPage);

            if (collected > 0)
            {
                Console.WriteLine($"  ✓ Collected {collected} samples for {showElem.Name}");
            }
            else
            {
                Console.WriteLine($"  ⚠ No samples found for {showElem.Name}");
            }

            await SaveCheckpointAsync(showIndex + 1, 1);
        }

        Console.WriteLine($"\nFinished. Total entries: {_writer.Total}");
    }

    private async Task<int> FetchByTmdbIdAsync(int showIndex, int tmdbId, string showName, int startPage)
    {
        int collected = 0;
        int page = startPage;

        while (collected < _perShowLimit)
        {
            var resp = await HttpRetryPolicy.ExecuteAsync(
                () => _client.GetSubtitlesByTmdbIdAsync(tmdbId, page),
                _baseDelayMs);

            if (resp == null || resp.Data.Length == 0) break;

            foreach (var item in resp.Data)
            {
                if (collected >= _perShowLimit) break;
                await _writer.WriteEntryAsync(item);
                collected++;
            }

            page++;
            await SaveCheckpointAsync(showIndex, page);
            await Task.Delay(_baseDelayMs);
        }

        return collected;
    }

    private async Task SaveCheckpointAsync(int showIndex, int nextPage)
    {
        await CheckpointStore.SaveAsync(
            _checkpointFile,
            new Checkpoint
            {
                ShowIndex = showIndex,
                NextPage = nextPage,
                TotalWritten = _writer.Total
            });
    }
}
