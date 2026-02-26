using System.Text.Json.Serialization;

namespace MLModel_TrainingDataApp.OpenSubtitles;

public class OpenSubtitlesSubtitlesResponse
{
    [JsonPropertyName("data")]        public OpenSubtitlesSubtitleItem[] Data { get; set; } = [];
    [JsonPropertyName("total_count")] public int TotalCount                  { get; set; }
    [JsonPropertyName("total_pages")] public int TotalPages                  { get; set; }
}

public class OpenSubtitlesSubtitleItem
{
    [JsonPropertyName("attributes")] public OpenSubtitlesSubtitleAttributes? Attributes { get; set; }
}

public class OpenSubtitlesSubtitleAttributes
{
    [JsonPropertyName("release")]         public string?                      Release        { get; set; }
    [JsonPropertyName("feature_details")] public OpenSubtitlesFeatureDetails? FeatureDetails { get; set; }
}

public class OpenSubtitlesFeatureDetails
{
    [JsonPropertyName("parent_title")]   public string? ParentTitle   { get; set; }
    [JsonPropertyName("title")]          public string? EpisodeTitle  { get; set; }
    [JsonPropertyName("season_number")]  public int?     SeasonNumber  { get; set; }
    [JsonPropertyName("episode_number")] public int?     EpisodeNumber { get; set; }
    [JsonPropertyName("year")]           public int?    Year          { get; set; }
}

public class OpenSubtitlesPopularShow
{
    public string Title     { get; init; } = "";
    public string FeatureId { get; init; } = "";
}
