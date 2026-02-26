using System.Text.Json;
using System.Text.Json.Serialization;

namespace MLModel_TrainingDataApp;

record Checkpoint
{
    [JsonPropertyName("show_index")]    public int ShowIndex    { get; init; } = 0;
    [JsonPropertyName("next_page")]     public int NextPage     { get; init; } = 1;
    [JsonPropertyName("total_written")] public int TotalWritten { get; init; } = 0;
}

static class CheckpointStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static async Task<Checkpoint?> LoadAsync(string path)
    {
        if (!File.Exists(path)) return null;
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<Checkpoint>(stream);
    }

    public static async Task SaveAsync(string path, Checkpoint checkpoint)
    {
        await using var stream = File.Open(path, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(stream, checkpoint, JsonOptions);
    }

    public static void Delete(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }
}
