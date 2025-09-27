using System.Text.RegularExpressions;
using System.IO.Abstractions;
using MediaOrganizer.Models;

namespace MediaOrganizer.Services;

public class TvShowEpisodeParser : ITvShowEpisodeParser
{
    private static readonly Regex StandardSxxExxPattern = new(
        @"^(?<showName>.+?)(?:\.(?<year>\d{4}))?\.S(?<season>\d{1,2})E(?<episode>\d{1,2})(?:\.(?<episodeTitle>[A-Za-z][A-Za-z\s]*[A-Za-z]))?\.(?:\d{4}p|1080p|720p|480p|REPACK|WEB|BluRay|ATVP|WEB-DL|DD|H\.?264|x264|h264|ETHEL|EZTVx|mkv|mp4|avi|playWEB|\[.*?\]|Atmos|5\.1|successfulcrab)", 
        RegexOptions.IgnoreCase);
    
    private static readonly Regex YearBeforeSxxExxPattern = new(
        @"^(?<showName>.+?)\s+(?<year>\d{4})\s+S(?<season>\d{1,2})E(?<episode>\d{1,2})\s+(?<episodeTitle>[A-Za-z][A-Za-z\s]*[A-Za-z])\s+(?:\d{4}p|1080p|720p|480p|REPACK|WEB|BluRay|ATVP|WEB-DL|DD|H\.?264|x264|h264|ETHEL|EZTVx|mkv|mp4|avi|playWEB|\[.*?\]|Atmos|5\.1)", 
        RegexOptions.IgnoreCase);
    
    private static readonly Regex YearInParenthesesPattern = new(
        @"^(?<showName>.+?)\s+\((?<year>\d{4})\)\s+S(?<season>\d{1,2})E(?<episode>\d{1,2})(?:\s+(?<episodeTitle>[A-Za-z][A-Za-z\s]*[A-Za-z]))?(?:\.|$)", 
        RegexOptions.IgnoreCase);
    
    private static readonly Regex SeasonXEpisodePattern = new(
        @"^(?<showName>.+?)\s+(?<season>\d{1,2})x(?<episode>\d{1,2})\s+(?<episodeTitle>[A-Za-z][A-Za-z\s]*[A-Za-z])(?:\.|$)", 
        RegexOptions.IgnoreCase);
    
    private static readonly Regex SpacedSxxExxWithTitlePattern = new(
        @"^(?<showName>.+?)\s+S(?<season>\d{1,2})E(?<episode>\d{1,2})\s+(?<episodeTitle>[A-Za-z][A-Za-z\s]*[A-Za-z])\s+(?:\d{4}p|1080p|720p|480p|REPACK|WEB|BluRay|ATVP|WEB-DL|DD|H\.?264|x264|h264|AMZN|DDP\d|FLUX|\[.*?\]|Atmos)", 
        RegexOptions.IgnoreCase);
    
    private static readonly Regex DashedSxxExxWithTitlePattern = new(
        @"^(?<showName>.+?)\s+-\s+S(?<season>\d{1,2})E(?<episode>\d{1,2})\s+-\s+(?<episodeTitle>.+?)(?:\.[^.]+)?$", 
        RegexOptions.IgnoreCase);
    
    private static readonly Regex SpacedSxxExxWithQualityPattern = new(
        @"^(?<showName>.+?)\s+S(?<season>\d{1,2})E(?<episode>\d{1,2})\s+(?:\d{4}p|1080p|720p|480p|4K|UHD|HDR|WEB|BluRay|HDTV|BDRip|DVDRip)(?:\.[^.]+)?$", 
        RegexOptions.IgnoreCase);
    
    private static readonly Regex DashedSxxExxPattern = new(
        @"^(?<showName>.+?)\s+-\s+S(?<season>\d{1,2})E(?<episode>\d{1,2})(?:\.[^.]+)?$", 
        RegexOptions.IgnoreCase);

    private static readonly Regex[] AllPatterns = [
        StandardSxxExxPattern,
        YearBeforeSxxExxPattern,
        YearInParenthesesPattern,
        SeasonXEpisodePattern,
        SpacedSxxExxWithTitlePattern,
        DashedSxxExxWithTitlePattern,
        SpacedSxxExxWithQualityPattern,
        DashedSxxExxPattern
    ];

    public bool CanParse(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return false;

        return AllPatterns.Any(pattern => pattern.IsMatch(filename));
    }

    public TvShowEpisode Parse(IFileInfo fileInfo)
    {
        string filename = fileInfo.Name;
        
        if (!CanParse(filename))
        {
            return new TvShowEpisode(fileInfo);
        }

        foreach (var pattern in AllPatterns)
        {
            var match = pattern.Match(filename);
            if (match.Success)
            {
                string showName = CleanShowName(match.Groups["showName"].Value);
                int season = int.Parse(match.Groups["season"].Value);
                int episode = int.Parse(match.Groups["episode"].Value);
                string episodeTitle = CleanEpisodeTitle(match.Groups["episodeTitle"].Value);
                int? year = match.Groups["year"].Success
                    ? int.Parse(match.Groups["year"].Value) 
                    : null;

                var tvShowEpisode = new TvShowEpisode(fileInfo);
                tvShowEpisode.TvShowName = showName;
                tvShowEpisode.Season = season;
                tvShowEpisode.Episode = episode;
                tvShowEpisode.Title = episodeTitle;
                tvShowEpisode.Year = year;

                return tvShowEpisode;
            }
        }

        return new TvShowEpisode(fileInfo);
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