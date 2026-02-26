using System.Text.Json.Serialization;

namespace MLModel_TrainingDataApp.Models;

public record ShowEntry
{
    [JsonPropertyName("tmdb_id")]
    public int TmdbId { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("first_air_date")]
    public string? FirstAirDate { get; init; }

    [JsonPropertyName("popularity")]
    public double? Popularity { get; init; }
}
