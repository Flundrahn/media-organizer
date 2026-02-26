using System.Net.Http.Json;
using System.Text.Json;

namespace MLModel_TrainingDataApp.OpenSubtitles;

public class OpenSubtitlesClient(HttpClient httpClient)
{
    private const string BaseUrl = "https://api.opensubtitles.com/api/v1";

    public static OpenSubtitlesClient Create(string apiKey)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Api-Key", apiKey);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "MediaOrganizerTraining v1.0");
        return new OpenSubtitlesClient(httpClient);
    }

    /// <summary>
    /// Get subtitles by TMDB ID (preferred method for TV shows).
    /// This is the most direct way to get subtitles when you have a TMDB identifier.
    /// </summary>
    public async Task<OpenSubtitlesSubtitlesResponse?> GetSubtitlesByTmdbIdAsync(int tmdbId, int page = 1)
    {
        return await httpClient.GetFromJsonAsync<OpenSubtitlesSubtitlesResponse>(
            $"{BaseUrl}/subtitles?parent_tmdb_id={tmdbId}&type=episode&page={page}");
    }

    /// <summary>
    /// Get subtitles by OpenSubtitles feature ID.
    /// Feature ID is OpenSubtitles' internal identifier.
    /// Note: Using TMDB ID directly is preferred when available.
    /// </summary>
    public async Task<OpenSubtitlesSubtitlesResponse?> GetSubtitlesByShowAsync(string featureId, int page = 1)
    {
        return await httpClient.GetFromJsonAsync<OpenSubtitlesSubtitlesResponse>(
            $"{BaseUrl}/subtitles?parent_feature_id={featureId}&page={page}");
    }

    /// <summary>
    /// Get subtitles by IMDB parent ID.
    /// NOTE: This method is no longer used in the current workflow which uses tmdb_id directly.
    /// Kept for backward compatibility if needed in the future.
    /// </summary>
    [Obsolete("Use GetSubtitlesByTmdbIdAsync instead. Direct TMDB lookup is more efficient.")]
    public async Task<OpenSubtitlesSubtitlesResponse?> GetSubtitlesByParentImdbIdAsync(int parentImdbId, int page = 1)
    {
        return await httpClient.GetFromJsonAsync<OpenSubtitlesSubtitlesResponse>(
            $"{BaseUrl}/subtitles?parent_imdb_id={parentImdbId}&type=episode&page={page}");
    }

    public async Task<OpenSubtitlesPopularShow[]> GetPopularShowsAsync()
    {
        var raw = await httpClient.GetStringAsync($"{BaseUrl}/discover/popular?type=episode");
        return JsonDocument.Parse(raw)
            .RootElement.GetProperty("data")
            .EnumerateArray()
            .Select(s => new OpenSubtitlesPopularShow
            {
                Title     = s.GetProperty("attributes").GetProperty("original_title").GetString() ?? "",
                FeatureId = s.GetProperty("attributes").GetProperty("feature_id").GetString()    ?? ""
            })
            .ToArray();
    }

    public async Task<OpenSubtitlesSubtitlesResponse?> GetLatestEpisodeSubtitlesAsync()
    {
        return await httpClient.GetFromJsonAsync<OpenSubtitlesSubtitlesResponse>(
            $"{BaseUrl}/discover/latest?type=episode");
    }
}
