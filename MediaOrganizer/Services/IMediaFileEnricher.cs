using MediaOrganizer.Models;

namespace MediaOrganizer.Services;

public interface IMediaFileEnricher<TMediaFile> where TMediaFile : IMediaFile
{
    Task<TvEpisodeEnrichmentResult> EnrichAsync(TMediaFile mediaFiles);

    Task<IEnumerable<TvEpisodeEnrichmentResult>> EnrichAllAsync(IEnumerable<TMediaFile> mediaFiles);
}
