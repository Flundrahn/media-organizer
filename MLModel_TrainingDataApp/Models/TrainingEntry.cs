using System.Text.Json.Serialization;

namespace MLModel_TrainingDataApp.Models;

public record TrainingEntry(
    [property: JsonPropertyName("filename")] string Filename,
    [property: JsonPropertyName("show_name")] string ShowName,
    [property: JsonPropertyName("episode_name")] string EpisodeName,
    [property: JsonPropertyName("season_number")] int SeasonNumber,
    [property: JsonPropertyName("episode_number")] int EpisodeNumber,
    [property: JsonPropertyName("year")] int? Year
);
