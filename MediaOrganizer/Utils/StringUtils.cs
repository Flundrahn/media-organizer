namespace MediaOrganizer.Utils;

public class StringUtils
{
    private readonly string[] _smallWords = ["a", "an", "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by"];

    public string ToTitleCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleCaseWords = new List<string>();

        foreach (var word in words)
        {
            // TODO: test if this ToUpper can be removed due to CapitalizeFirstWord later
            if (word.Length == 1)
            {
                titleCaseWords.Add(word.ToUpper());
            }
            else if (IsSmallWord(word))
            {
                titleCaseWords.Add(word.ToLower());
            }
            else
            {
                titleCaseWords.Add(char.ToUpper(word[0]) + word.Substring(1).ToLower());
            }
        }

        if (titleCaseWords.Count > 0)
        {
            titleCaseWords[0] = CapitalizeFirstWord(titleCaseWords[0]);
        }

        return string.Join(' ', titleCaseWords);
    }

    private bool IsSmallWord(string word)
    {
        return Array.Exists(_smallWords, w => string.Equals(w, word, StringComparison.OrdinalIgnoreCase));
    }

    private string CapitalizeFirstWord(string word)
    {
        if (string.IsNullOrEmpty(word))
            return word;

        return char.ToUpper(word[0]) + word.Substring(1);
    }
}