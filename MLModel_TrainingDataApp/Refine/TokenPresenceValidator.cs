using System.Text.RegularExpressions;
using MLModel_TrainingDataApp.Models;

namespace MLModel_TrainingDataApp.Refine;

/// <summary>
/// Validates that show name tokens are present in the filename.
/// This class ONLY validates - it does NOT mutate data.
/// Show name cleaning is handled by RefinedTrainingEntry.CleanShowName().
/// </summary>
static class TokenPresenceValidator
{
    /// <summary>
    /// Validate entries by checking if show name tokens appear in filename.
    /// Returns valid and invalid entries. Does NOT mutate the entries.
    /// </summary>
    public static (List<RefinedTrainingEntry> Valid, List<RefinedTrainingEntry> Invalid) ValidateEntries(List<RefinedTrainingEntry> entries)
    {
        var valid = new List<RefinedTrainingEntry>();
        var invalid = new List<RefinedTrainingEntry>();

        foreach (var entry in entries)
        {
            if (IsShowNamePresent(entry.Filename, entry))
            {
                valid.Add(entry);
            }
            else
            {
                invalid.Add(entry);
            }
        }

        double skipPercent = entries.Count > 0
            ? 100.0 * invalid.Count / entries.Count
            : 0;
        Console.WriteLine($"Token-presence validation: kept {valid.Count}, skipped {invalid.Count} ({skipPercent:F1}%)");

        return (valid, invalid);
    }

    /// <summary>
    /// Check if show name tokens are present in filename.
    /// Both filename and show name are cleaned using the same logic.
    /// Allows for fuzzy matching of tokens (e.g., "Dale" matches "Dales").
    /// </summary>
    private static bool IsShowNamePresent(string filename, RefinedTrainingEntry entry)
    {
        string cleanedFilename = CleanString(filename);
        List<string> filenameTokens = cleanedFilename.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        List<string> showTokens = entry.GetShowNameTokens().ToList();

        if (showTokens.Count == 0) return false;

        // Check if show name tokens appear as a consecutive subsequence in filename (exact match)
        if (FindSubsequence(filenameTokens, showTokens, StringComparison.OrdinalIgnoreCase))
            return true;

        // Also check without "the" if the show name starts with it
        if (showTokens.Count > 1 && showTokens[0].Equals("the", StringComparison.OrdinalIgnoreCase))
        {
            var tokensWithoutThe = showTokens.Skip(1).ToList();
            if (FindSubsequence(filenameTokens, tokensWithoutThe, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        // Fuzzy matching: allow tokens that are very similar (handles cases like "Dale" vs "Dales")
        if (FindSubsequenceFuzzy(filenameTokens, showTokens))
            return true;

        return false;
    }

    /// <summary>
    /// Clean a string using the same logic as RefinedTrainingEntry.CleanShowName().
    /// This ensures consistent cleaning between show names and filenames for comparison.
    /// </summary>
    private static string CleanString(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var cleaned = input;
        
        // Handle unicode apostrophes first by converting to regular apostrophe
        cleaned = cleaned.Replace("'", "'");
        
        // Apply the same transformations as CleanShowName
        cleaned = cleaned
            .Replace(":", " ")      // colons
            .Replace("/", " ")      // slashes
            .Replace("&", "And")    // ampersands become "And"
            .Replace("-", " ")      // hyphens
            .Replace("_", " ")      // underscores
            .Replace(".", " ")      // periods
            .Replace("!", " ")      // exclamation marks
            .Replace("?", " ")      // question marks
            .Replace("'", "");      // apostrophes (including possessives)

        // Collapse multiple spaces to single space and trim
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        return cleaned;
    }

    /// <summary>
    /// Find if needle tokens appear as a consecutive subsequence in haystack tokens.
    /// Uses case-insensitive comparison.
    /// </summary>
    private static bool FindSubsequence(List<string> haystack, List<string> needle, StringComparison comparison)
    {
        if (needle.Count == 0) return false;

        for (int i = 0; i <= haystack.Count - needle.Count; i++)
        {
            bool match = true;
            for (int j = 0; j < needle.Count; j++)
            {
                if (!haystack[i + j].Equals(needle[j], comparison))
                {
                    match = false;
                    break;
                }
            }
            if (match) return true;
        }

        return false;
    }

    /// <summary>
    /// Find if needle tokens appear as a consecutive subsequence with fuzzy matching.
    /// Allows tokens that start the same (e.g., "Dale" matches "Dales").
    /// Requires at least 80% of tokens to match.
    /// </summary>
    private static bool FindSubsequenceFuzzy(List<string> haystack, List<string> needle)
    {
        if (needle.Count == 0) return false;

        // Only use fuzzy matching for small show names to avoid false positives
        if (needle.Count < 2) return false;

        for (int i = 0; i <= haystack.Count - needle.Count; i++)
        {
            int matchCount = 0;
            for (int j = 0; j < needle.Count; j++)
            {
                string haystackToken = haystack[i + j];
                string needleToken = needle[j];
                
                // Tokens match if they're equal or one starts with the other
                if (haystackToken.Equals(needleToken, StringComparison.OrdinalIgnoreCase) ||
                    haystackToken.StartsWith(needleToken, StringComparison.OrdinalIgnoreCase) ||
                    needleToken.StartsWith(haystackToken, StringComparison.OrdinalIgnoreCase))
                {
                    matchCount++;
                }
            }

            // Accept if at least 80% of tokens match
            if (matchCount >= (needle.Count * 0.8))
            {
                return true;
            }
        }

        return false;
    }
}
