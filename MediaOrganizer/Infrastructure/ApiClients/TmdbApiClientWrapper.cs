using MediaOrganizer.Utils;
using Microsoft.Extensions.Logging;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.TvShows;

namespace MediaOrganizer.Infrastructure.ApiClients
{
    /// <summary>
    /// Thin wrapper around TMDbLib's TMDbClient to make it easy to mock in unit tests and keep
    /// user-facing classes free from TMDbLib types.
    /// </summary>
    public interface ITmdbApiClient
    {
        Task<SearchContainer<SearchTv>> SearchTvShowAsync(string query, int page = 1, bool includeAdult = false, int year = 0, CancellationToken cancellationToken = default);
        Task<TvEpisode?> GetTvEpisodeAsync(int tvShowId, int seasonNumber, int episodeNumber);
    }

    public class TmdbApiClientWrapper : ITmdbApiClient
    {
        private readonly ILogger<TmdbApiClientWrapper> _logger;
        private readonly TMDbClient _client;

        public TmdbApiClientWrapper(ILogger<TmdbApiClientWrapper> logger, TMDbClient apiClient)
        {
            _logger = logger;
            _client = apiClient;
        }

        public async Task<SearchContainer<SearchTv>> SearchTvShowAsync(
            string query,
            int page = 1,
            bool includeAdult = false,
            int year = 0,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Searching TV show with query='{Query}', page={Page}, includeAdult={IncludeAdult}, year={Year}",
                query,
                page,
                includeAdult,
                year);

            // Using a .NET Standard library means don't know if returned reference types are nullable or not, check to fail fast.
            SearchContainer<SearchTv>? response = await _client.SearchTvShowAsync(
                query,
                page,
                includeAdult,
                year,
                cancellationToken);

            NullGuard.ThrowIfNull(response);
            NullGuard.ThrowIfNull(response.Results);

            _logger.LogInformation(
                "Searching TV show completed: found {Count} results on page {Page}",
                response.Results.Count,
                response.Page);

            foreach (SearchTv searchResult in response.Results)
            {
                _logger.LogDebug("Found TV show TMDB ID {TmdbId} {ShowName}", searchResult.Id, searchResult.Name);
            }

            return response!;
        }

        /// <summary>
        /// If TvEpisode is not found null will be returned.
        /// </summary>
        public async Task<TvEpisode?> GetTvEpisodeAsync(int tvShowId, int seasonNumber, int episodeNumber)
        {
            _logger.LogInformation(
                "Getting TV episode for tvShowId={TvShowId}, season={SeasonNumber}, episode={EpisodeNumber}",
                tvShowId,
                seasonNumber,
                episodeNumber);

            TvEpisode? episode = await _client.GetTvEpisodeAsync(
                tvShowId,
                seasonNumber,
                episodeNumber);

            if (episode is null)
            {
                _logger.LogWarning(
                    "TV episode not found for tvShowId={TvShowId}, season={SeasonNumber}, episode={EpisodeNumber}",
                    tvShowId,
                    seasonNumber,
                    episodeNumber);

                return null;
            }

            _logger.LogInformation(
                "Getting TV episode completed: episodeId={EpisodeId}, name='{EpisodeName}'",
                episode.Id,
                episode.Name);

            return episode;
        }
    }
}