using System.Text.RegularExpressions;
using MediaOrganizer.Models;

namespace MediaOrganizer.Services;

public class TvShowEpisodeParser : ITvShowEpisodeParser
{
    private readonly Regex[] _patterns = [
        // Standard SxxExx format (e.g., Show.Name.S01E07.quality.tags.mkv or Show.Name.2022.S01E07.quality.mkv)
        new Regex(@"^(?<showName>.+?)(?:\.(?<year>\d{4}))?\.S(?<season>\d{1,2})E(?<episode>\d{1,2})(?:\.(?<episodeTitle>[A-Za-z][A-Za-z\s]*[A-Za-z]))?\.(?:\d{4}p|1080p|720p|480p|REPACK|WEB|BluRay|ATVP|WEB-DL|DD|H\.?264|x264|h264|ETHEL|EZTVx|mkv|mp4|avi|playWEB|\[.*?\]|Atmos|5\.1|successfulcrab)", RegexOptions.IgnoreCase),
        
        // Format with year after show name (e.g., Foundation 2021 S03E09 Episode Title quality.mkv)
        new Regex(@"^(?<showName>.+?)\s+(?<year>\d{4})\s+S(?<season>\d{1,2})E(?<episode>\d{1,2})\s+(?<episodeTitle>[A-Za-z][A-Za-z\s]*[A-Za-z])\s+(?:\d{4}p|1080p|720p|480p|REPACK|WEB|BluRay|ATVP|WEB-DL|DD|H\.?264|x264|h264|ETHEL|EZTVx|mkv|mp4|avi|playWEB|\[.*?\]|Atmos|5\.1)", RegexOptions.IgnoreCase),
        
        // Format with year in parentheses (e.g., Show Name (2005) S01E01.mkv or Show Name (2005) S01E01 Episode Title.mp4)
        new Regex(@"^(?<showName>.+?)\s+\((?<year>\d{4})\)\s+S(?<season>\d{1,2})E(?<episode>\d{1,2})(?:\s+(?<episodeTitle>[A-Za-z][A-Za-z\s]*[A-Za-z]))?(?:\.|$)", RegexOptions.IgnoreCase),
        
        // SeasonXEpisode format (e.g., Show Name 1x01 Episode Title.mkv)
        new Regex(@"^(?<showName>.+?)\s+(?<season>\d{1,2})x(?<episode>\d{1,2})\s+(?<episodeTitle>[A-Za-z][A-Za-z\s]*[A-Za-z])(?:\.|$)", RegexOptions.IgnoreCase)
    ];

    public bool CanParse(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return false;

        return _patterns.Any(pattern => pattern.IsMatch(filename));
    }

    public EpisodeInfo Parse(string filename)
    {
        if (!CanParse(filename))
            return new EpisodeInfo();

        foreach (var pattern in _patterns)
        {
            var match = pattern.Match(filename);
            if (match.Success)
            {
                var showName = CleanShowName(match.Groups["showName"].Value);
                var season = int.Parse(match.Groups["season"].Value);
                var episode = int.Parse(match.Groups["episode"].Value);
                var episodeTitle = CleanEpisodeTitle(match.Groups["episodeTitle"].Value);
                var year = match.Groups["year"].Success ? int.Parse(match.Groups["year"].Value) : (int?)null;

                return new EpisodeInfo
                {
                    ShowName = showName,
                    Season = season,
                    Episode = episode,
                    EpisodeTitle = episodeTitle,
                    Year = year
                };
            }
        }

        return new EpisodeInfo();
    }

    private static string CleanShowName(string showName)
    {
        if (string.IsNullOrWhiteSpace(showName))
            return "";

        // Replace dots with spaces and clean up
        var cleaned = showName.Replace(".", " ")
                              .Replace("_", " ")
                              .Trim();

        // Remove extra spaces
        cleaned = Regex.Replace(cleaned, @"\s+", " ");

        // Convert to proper case - capitalize first letter of each word
        var words = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + (words[i].Length > 1 ? words[i][1..].ToLower() : "");
            }
        }

        return string.Join(" ", words);
    }

    private static string CleanEpisodeTitle(string episodeTitle)
    {
        if (string.IsNullOrWhiteSpace(episodeTitle))
            return "";

        // Clean up episode title
        var cleaned = episodeTitle.Replace(".", " ")
                                 .Replace("_", " ")
                                 .Trim();

        // Remove extra spaces
        cleaned = Regex.Replace(cleaned, @"\s+", " ");

        // Return empty string if it looks like quality info or metadata (single letters, numbers, or quality terms)
        if (string.IsNullOrWhiteSpace(cleaned) || 
            cleaned.Length <= 2 || 
            Regex.IsMatch(cleaned, @"^\d+$") ||  // Just numbers
            Regex.IsMatch(cleaned, @"^[A-Z]$") || // Single letter
            Regex.IsMatch(cleaned, @"^\d{4}p$|^(1080p|720p|480p|REPACK|WEB|BluRay|ATVP|WEB-DL|DD|H\.?264|x264|h264)$", RegexOptions.IgnoreCase))
            return "";

        return cleaned;
    }
}