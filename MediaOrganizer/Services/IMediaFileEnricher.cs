using MediaOrganizer.Models;
using MediaOrganizer.Utils;

namespace MediaOrganizer.Services;

public interface IMediaFileEnricher<TMediaFile> where TMediaFile : IMediaFile
{
    Task<ResultBase> EnrichAsync(TMediaFile mediaFiles);

    Task EnrichAllAsync(IEnumerable<TMediaFile> mediaFiles);
}
