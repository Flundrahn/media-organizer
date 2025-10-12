namespace MediaOrganizer.Infrastructure.ApiClients;

public class TmdbApiConfig
{
    public const string SectionName = "TmdbApi";

    private string _apiKey = string.Empty;
    private string _validationError = string.Empty;

    public string ApiKey
    {
        get
        {
            if (!IsApiKeyValid(_apiKey))
            {
                throw new InvalidOperationException(_validationError);
            }
            return _apiKey;
        }
        set
        {
            if (!IsApiKeyValid(value))
            {
                throw new ArgumentException(_validationError, nameof(value));
            }
            _apiKey = value;
        }
    }

    private bool IsApiKeyValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _validationError = "ApiKey cannot be null or whitespace.";
            return false;
        }
        if (!value.All(char.IsLetterOrDigit))
        {
            _validationError = "ApiKey must contain only letters and numbers.";
            return false;
        }
        return true;
    }
}
