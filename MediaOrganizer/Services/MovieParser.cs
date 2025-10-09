using System.Globalization;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using MediaOrganizer.Models;

namespace MediaOrganizer.Services;

// TODO possibly just remove the wild card matching of the last group.
public class MovieParser : IMediaFileParser
{
    // Example: "Grandmas Boy 2006 UNRATED 1080p BluRay HEVC x265 5.1 BONE.mkv"
    private static readonly Regex MovieWithYearAndQualityPattern = new(
        @"^(?<title>.+?)\s+(?<year>\d{4})\s+(?:UNRATED\s+)?(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)(?:\s+.+?)?$",
        RegexOptions.IgnoreCase);

    // Example: "A Brilliant Young Mind (2014) (1080p BluRay x265 10bit Tigole).mkv"
    private static readonly Regex MovieWithYearInParenthesesPattern = new(
        @"^(?<title>.+?)\s+\((?<year>\d{4})\)\s+\((?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)(?:\s+[^)]*)?",
        RegexOptions.IgnoreCase);

    // Example: "Clash.Of.The.Titans.1981.1080p.BluRay.x264-[YTS.AM].mp4"
    private static readonly Regex MovieWithDotsPattern = new(
        @"^(?<title>(?:[A-Za-z0-9]+\.)*[A-Za-z0-9]+)\.(?<year>\d{4})\.(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)(?:\..*)?$",
        RegexOptions.IgnoreCase);

    // Example: "Interstellar 1080p.mkv", "Thor Ragnarok 1080p.mkv" 
    private static readonly Regex MovieSimpleWithQualityPattern = new(
        @"^(?<title>.+?)\s+(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)(?:\s|$)",
        RegexOptions.IgnoreCase);

    // Example: "Superman 2025", "Wonder Woman 1984"
    private static readonly Regex MovieWithYearPattern = new(
        @"^(?<title>.+?)\s+(?<year>\d{4})(?:\s|$)",
        RegexOptions.IgnoreCase);

    // Example: "Thunderbolts.2025.Proper.1080p.WEB-DL.DDP5.1.x265-NeoNoir"
    private static readonly Regex MovieComplexDotsPattern = new(
        @"^(?<title>(?:[A-Za-z0-9]+\.)*[A-Za-z0-9]+)\.(?<year>\d{4})\.(?:[A-Za-z0-9\-\.]+\.)*(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)(?:.*)?$",
        RegexOptions.IgnoreCase);

    // Example: "Solo A Star Wars Story 2160p.mkv" - title with spaces followed by quality
    private static readonly Regex MovieLongTitleWithQualityPattern = new(
        @"^(?<title>(?:[A-Za-z]+\s+){2,}[A-Za-z]+)\s+(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)(?:\s|$)",
        RegexOptions.IgnoreCase);

    // Example: "Samsara 1080p.mkv", "Tolkien 1080p.mp4" - simple title with quality
    private static readonly Regex MovieSimpleTitlePattern = new(
        @"^(?<title>[A-Za-z][A-Za-z\s]*[A-Za-z])(?:\s+\d{3,4}p|\s+4K|\s+UHD|\s+HDR|$)",
        RegexOptions.IgnoreCase);

    private static readonly Regex[] AllPatterns = [
        MovieWithYearAndQualityPattern,
        MovieWithYearInParenthesesPattern,
        MovieWithDotsPattern,
        MovieComplexDotsPattern,
        MovieLongTitleWithQualityPattern,
        MovieSimpleWithQualityPattern,
        MovieWithYearPattern,
        MovieSimpleTitlePattern
    ];

    public bool CanParse(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return false;

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

        foreach (var pattern in AllPatterns)
        {
            if (pattern.IsMatch(nameWithoutExtension))
                return true;
        }

        return false;
    }

    public IMediaFile Parse(IFileInfo fileInfo)
    {
        var movie = new Movie(fileInfo);
        var filename = fileInfo.Name;
        
        // Remove file extension for parsing
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

        foreach (var pattern in AllPatterns)
        {
            var match = pattern.Match(nameWithoutExtension);
            if (match.Success)
            {
                // Extract title and clean it up
                var title = ExtractAndCleanTitle(match.Groups["title"].Value);
                if (!string.IsNullOrWhiteSpace(title))
                {
                    movie.Title = title;
                }

                // Extract year if present
                if (match.Groups["year"].Success && int.TryParse(match.Groups["year"].Value, out int year))
                {
                    movie.Year = year;
                }

                // Extract quality if present
                if (match.Groups["quality"].Success)
                {
                    movie.Quality = match.Groups["quality"].Value;
                }
                break; // Use the first successful match
            }
        }

        return movie;
    }

   private static string ExtractAndCleanTitle(string rawTitle)
    {
        if (string.IsNullOrWhiteSpace(rawTitle))
            return string.Empty;

        // Replace dots with spaces for dotted titles
        var title = rawTitle.Replace('.', ' ');
        
        // Clean up extra whitespace
        title = Regex.Replace(title, @"\s+", " ").Trim();
        
        // Capitalize properly (basic title case)
        return ToTitleCase(title);
        // return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title);
    }

    private static string ToTitleCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleCaseWords = new List<string>();

        foreach (var word in words)
        {
            if (word.Length == 1)
            {
                titleCaseWords.Add(word.ToUpper());
            }
            else if (IsSmallWord(word.ToLower()))
            {
                titleCaseWords.Add(word.ToLower());
            }
            else
            {
                titleCaseWords.Add(char.ToUpper(word[0]) + word.Substring(1).ToLower());
            }
        }

        // Always capitalize the first word
        if (titleCaseWords.Count > 0)
        {
            titleCaseWords[0] = CapitalizeFirstWord(titleCaseWords[0]);
        }

        return string.Join(' ', titleCaseWords);
    }

    private static bool IsSmallWord(string word)
    {
        var smallWords = new[] { "a", "an", "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by" };
        return smallWords.Contains(word);
    }

    private static string CapitalizeFirstWord(string word)
    {
        if (string.IsNullOrEmpty(word))
            return word;
        
        return char.ToUpper(word[0]) + word.Substring(1);
    }
}