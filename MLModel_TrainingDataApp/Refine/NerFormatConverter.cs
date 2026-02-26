using System.Text;
using System.Text.RegularExpressions;
using MLModel_TrainingDataApp.Models;

namespace MLModel_TrainingDataApp.Refine;

static class NerFormatConverter
{
    /// <summary>
    /// Convert refined training entries to NER token format (CoNLL-style BIO).
    /// Output: token\tlabel per line, blank line between examples.
    /// </summary>
    public static void ConvertToNerFormat(List<RefinedTrainingEntry> entries, string outputPath)
    {
        using var writer = new StreamWriter(outputPath, false, Encoding.UTF8);
        int successCount = 0;
        int failCount = 0;

        foreach (var entry in entries)
        {
            var tokens = Tokenize(entry.Filename);
            var labels = LabelTokens(tokens, entry);

            // Only write if we successfully labeled at least one B-SHOW
            if (labels.Any(l => l == "B-SHOW"))
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    writer.WriteLine($"{tokens[i]}\t{labels[i]}");
                }
                writer.WriteLine(); // blank line separator
                successCount++;
            }
            else
            {
                failCount++;
            }
        }

        Console.WriteLine($"Wrote {successCount} NER examples ({failCount} failed to label) to {outputPath}");
    }

    private static List<string> Tokenize(string filename)
    {
        // Split on dots, dashes, underscores, spaces
        return Regex.Split(filename, @"[\.\-_\s]+")
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();
    }

    private static List<string> LabelTokens(List<string> tokens, RefinedTrainingEntry entry)
    {
        var labels = Enumerable.Repeat("O", tokens.Count).ToList();

        var normalizedTokens = tokens.Select(NormalizeToken).ToList();
        var showTokens = entry.GetShowNameTokens().ToList();

        if (showTokens.Count == 0) return labels;

        var matchIndices = FindSubsequence(normalizedTokens, showTokens);

        if (matchIndices != null && matchIndices.Count > 0)
        {
            labels[matchIndices[0]] = "B-SHOW";
            for (int i = 1; i < matchIndices.Count; i++)
            {
                labels[matchIndices[i]] = "I-SHOW";
            }
        }

        return labels;
    }

    private static string NormalizeToken(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // Same normalization as used elsewhere
        var normalized = input
            .Replace("'", "")
            .Replace("'", "")
            .Replace(":", " ")
            .Replace("/", " ")
            .Replace("&", " ")
            .Replace("-", " ")
            .Replace("_", " ")
            .Replace(".", " ");

        normalized = Regex.Replace(normalized, @"[^\w\s]", " ");
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim().ToLowerInvariant();

        return normalized;
    }

    private static List<int>? FindSubsequence(List<string> haystack, List<string> needle)
    {
        if (needle.Count == 0) return null;

        // Try exact match first
        for (int i = 0; i <= haystack.Count - needle.Count; i++)
        {
            bool match = true;
            for (int j = 0; j < needle.Count; j++)
            {
                if (haystack[i + j] != needle[j])
                {
                    match = false;
                    break;
                }
            }
            if (match) return Enumerable.Range(i, needle.Count).ToList();
        }

        // Try without leading "the"
        if (needle.Count > 1 && needle[0] == "the")
        {
            var needleWithoutThe = needle.Skip(1).ToList();
            for (int i = 0; i <= haystack.Count - needleWithoutThe.Count; i++)
            {
                bool match = true;
                for (int j = 0; j < needleWithoutThe.Count; j++)
                {
                    if (haystack[i + j] != needleWithoutThe[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return Enumerable.Range(i, needleWithoutThe.Count).ToList();
            }
        }

        return null;
    }
}
