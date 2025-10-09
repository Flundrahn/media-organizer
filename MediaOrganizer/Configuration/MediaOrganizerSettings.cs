using System.Text.RegularExpressions;
using MediaOrganizer.Models;
using MediaOrganizer.Validations;

namespace MediaOrganizer.Configuration;

/// <summary>
/// Configuration settings for the Media Organizer application
/// </summary>
public class MediaOrganizerSettings
{
    private List<string> _validationErrors = [];
    private FileSystemValidator? _validator;
    private string _sourceDirectory = string.Empty;
    private string _destinationDirectory = string.Empty;
    private string _tvShowSourceDirectory = string.Empty;
    private string _tvShowDestinationDirectory = string.Empty;
    private string _movieSourceDirectory = string.Empty;
    private string _movieDestinationDirectory = string.Empty;

    public const string SectionName = "MediaOrganizer";

    // TODO: clean up "legacy" directories

    /// <summary>
    /// The source directory to scan for media files
    /// Automatically converts relative paths to absolute paths
    /// </summary>
    public string SourceDirectory
    {
        get => _sourceDirectory;
        set => _sourceDirectory = string.IsNullOrWhiteSpace(value) ? string.Empty : Path.GetFullPath(value);
    }

    /// <summary>
    /// The destination directory where organized files will be placed
    /// Automatically converts relative paths to absolute paths
    /// </summary>
    public string DestinationDirectory 
    { 
        get => _destinationDirectory;
        set => _destinationDirectory = string.IsNullOrWhiteSpace(value) ? string.Empty : Path.GetFullPath(value);
    }

    /// <summary>
    /// The source directory to scan for TV show files
    /// Automatically converts relative paths to absolute paths
    /// </summary>
    public string TvShowSourceDirectory 
    { 
        get => _tvShowSourceDirectory;
        set => _tvShowSourceDirectory = string.IsNullOrWhiteSpace(value) ? string.Empty : Path.GetFullPath(value);
    }

    /// <summary>
    /// The destination directory where organized TV show files will be placed
    /// Automatically converts relative paths to absolute paths
    /// </summary>
    public string TvShowDestinationDirectory 
    { 
        get => _tvShowDestinationDirectory;
        set => _tvShowDestinationDirectory = string.IsNullOrWhiteSpace(value) ? string.Empty : Path.GetFullPath(value);
    }

    /// <summary>
    /// The source directory to scan for movie files
    /// Automatically converts relative paths to absolute paths
    /// </summary>
    public string MovieSourceDirectory 
    { 
        get => _movieSourceDirectory;
        set => _movieSourceDirectory = string.IsNullOrWhiteSpace(value) ? string.Empty : Path.GetFullPath(value);
    }

    /// <summary>
    /// The destination directory where organized movie files will be placed
    /// Automatically converts relative paths to absolute paths
    /// </summary>
    public string MovieDestinationDirectory 
    { 
        get => _movieDestinationDirectory;
        set => _movieDestinationDirectory = string.IsNullOrWhiteSpace(value) ? string.Empty : Path.GetFullPath(value);
    }

    /// <summary>
    /// When true, only shows what would be done without actually moving files
    /// </summary>
    public bool DryRun { get; set; } = true;

    /// <summary>
    /// When true, includes all subdirectories when searching for media files
    /// </summary>
    public bool IncludeSubdirectories { get; set; } = true;

    /// <summary>
    /// The template used to generate relative paths for TV show episodes.
    /// Supports placeholders: {TvShowName}, {Season}, {Episode}, {Title}, {Year}
    /// The file extension from the original file is automatically appended.
    /// </summary>
    public string TvShowPathTemplate { get; set; } = string.Empty;

    /// <summary>
    /// The template used to generate relative paths for movies.
    /// Supports placeholders: {Title}, {Year}, {Quality}
    /// The file extension from the original file is automatically appended.
    /// </summary>
    public string MoviePathTemplate { get; set; } = string.Empty;

    /// <summary>
    /// List of video file extensions to include when scanning for media files.
    /// Extensions should include the dot (e.g., ".mp4", ".avi").
    /// Case-insensitive matching is used.
    /// </summary>
    public List<string> VideoFileExtensions { get; set; } = new List<string>();

    /// <summary>
    /// When true, automatically removes empty directories after finishing OrganizeFiles operation.
    /// Only removes directories that become empty as a result of the file organization process.
    /// Note: Directory cleanup can also be run manually through the application menu.
    /// </summary>
    public bool AutoCleanupEmptyDirectories { get; set; } = false;

    /// <summary>
    /// List of folder names to ignore when scanning for media files.
    /// These folders will be completely skipped during file discovery.
    /// Case-insensitive matching is used.
    /// Common examples: "Featurettes", "Extras", "Behind the Scenes", "Deleted Scenes"
    /// </summary>
    public List<string> IgnoredFolders { get; set; } = new List<string>();

    public ICollection<string> GetValidationErrors() => _validationErrors;

    // Set manually since options need empty ctor
    public void SetValidator(FileSystemValidator validator)
    {
        _validator = validator;
    }

    /// <summary>
    /// Validates the configuration settings
    /// </summary>
    /// <returns>True if configuration is valid</returns>
    
    // NOTE: Possibly extract validation to own class
    public bool IsValid()
    {
        if (_validator is null)
        {
            throw new InvalidOperationException("Validator must be set.");
        }

        _validationErrors.Clear();
        bool isValid = true;

        // bitwise AND operation, once false stays false
        isValid &= ValidateTvShowSourceDirectory();
        isValid &= ValidateTvShowDestinationDirectory();
        isValid &= ValidateMovieSourceDirectory();
        isValid &= ValidateMovieDestinationDirectory();
        isValid &= ValidateTvShowPathTemplate();
        isValid &= ValidateMoviePathTemplate();
        isValid &= ValidateVideoFileExtensions();
        isValid &= ValidateIgnoredFolders();

        return isValid;
    }

    private bool ValidateTvShowSourceDirectory()
    {
        return ValidateSourceDirectory(TvShowSourceDirectory, nameof(TvShowSourceDirectory));
    }

    private bool ValidateTvShowDestinationDirectory()
    {
        if (string.IsNullOrWhiteSpace(TvShowDestinationDirectory))
        {
            _validationErrors.Add($"{nameof(TvShowDestinationDirectory)} is required");
            return false;
        }
        
        if (!_validator!.DirectoryIsWriteable(TvShowDestinationDirectory))
        {
            _validationErrors.Add($"{nameof(TvShowDestinationDirectory)} is not writable or cannot be created");
            return false;
        }

        return true;
    }

    private bool ValidateMovieSourceDirectory()
    {
        return ValidateSourceDirectory(MovieSourceDirectory, nameof(MovieSourceDirectory));
    }

    private bool ValidateSourceDirectory(string directory, string directoryName)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            _validationErrors.Add($"{directoryName} is required");
            return false;
        }
        
        if (!_validator!.DirectoryExists(directory))
        {
            _validationErrors.Add($"{directoryName} does not exist: {directory}");
            return false;
        }

        return true;
    }

    private bool ValidateMovieDestinationDirectory()
    {
        if (string.IsNullOrWhiteSpace(MovieDestinationDirectory))
        {
            _validationErrors.Add($"{nameof(MovieDestinationDirectory)} is required");
            return false;
        }
        
        if (!_validator!.DirectoryIsWriteable(MovieDestinationDirectory))
        {
            _validationErrors.Add($"{nameof(MovieDestinationDirectory)} is not writable or cannot be created");
            return false;
        }

        return true;
    }

    private bool ValidateTvShowPathTemplate()
    {
        return ValidateMediaPathTemplate(TvShowPathTemplate, nameof(TvShowPathTemplate), TvShowEpisode.ValidPlaceholders);
    }

    private bool ValidateMoviePathTemplate()
    {
        return ValidateMediaPathTemplate(MoviePathTemplate, nameof(MoviePathTemplate), Movie.ValidPlaceholders);
    }

    private bool ValidateMediaPathTemplate(string template, string templateName, HashSet<string> validPlaceholders)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            _validationErrors.Add($"{templateName} is required");
            return false;
        }

        var isValid = true;

        // Validate placeholders in the template using the valid placeholders
        var placeholderMatches = Regex.Matches(template, @"\{([^}:]*)[^}]*\}");
        foreach (Match match in placeholderMatches)
        {
            var placeholderName = match.Groups[1].Value;
            if (!validPlaceholders.Contains(placeholderName))
            {
                _validationErrors.Add($"{templateName} contains invalid placeholder: {{{placeholderName}}}");
                isValid = false;
            }
        }

        // Validate path characters by removing placeholders and checking segments
        var templateWithoutPlaceholders = Regex.Replace(
            template,
            @"\{[^}]*\}",
            "X"); // Replace placeholders with a safe character

        var pathParts = templateWithoutPlaceholders.Split('/', '\\');
        if (_validator != null && !_validator.AreValidPathSegments(pathParts))
        {
            _validationErrors.Add($"{templateName} contains invalid path characters");
            isValid = false;
        }

        return isValid;
    }

    private bool ValidateVideoFileExtensions()
    {
        // Validate video file extensions
        if (VideoFileExtensions == null || VideoFileExtensions.Count == 0)
        {
            _validationErrors.Add("VideoFileExtensions must contain at least one extension");
            return false;
        }

        var isValid = true;
        foreach (var extension in VideoFileExtensions)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                _validationErrors.Add("VideoFileExtensions cannot contain empty or whitespace extensions");
                isValid = false;
                break;
            }

            if (!extension.StartsWith('.'))
            {
                _validationErrors.Add($"VideoFileExtensions must start with a dot: '{extension}'");
                isValid = false;
            }

            if (extension.Length < 2)
            {
                _validationErrors.Add($"VideoFileExtensions must have at least one character after the dot: '{extension}'");
                isValid = false;
            }

            if (_validator != null && !_validator.IsValidPathSegment(extension))
            {
                _validationErrors.Add($"VideoFileExtensions contains invalid characters: '{extension}'");
                isValid = false;
            }
        }

        return isValid;
    }

    private bool ValidateIgnoredFolders()
    {
        bool isValid = true;

        foreach (var folder in IgnoredFolders)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                _validationErrors.Add($"{nameof(IgnoredFolders)} cannot contain empty or whitespace folder names");
                isValid = false;
                continue;
            }

            // Check for invalid path characters
            var invalidChars = Path.GetInvalidFileNameChars();
            if (folder.IndexOfAny(invalidChars) != -1)
            {
                _validationErrors.Add($"{nameof(IgnoredFolders)} contains invalid characters in folder name: '{folder}'");
                isValid = false;
            }

            // Check for reserved Windows names (CON, PRN, etc.)
            var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
            if (reservedNames.Contains(folder.ToUpperInvariant()))
            {
                _validationErrors.Add($"{nameof(IgnoredFolders)} contains reserved folder name: '{folder}'");
                isValid = false;
            }
        }

        return isValid;
    }
}