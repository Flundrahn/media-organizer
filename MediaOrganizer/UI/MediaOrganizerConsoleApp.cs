using MediaOrganizer.Configuration;
using MediaOrganizer.IO;
using MediaOrganizer.Services;
using MediaOrganizer.Validations;
using Microsoft.Extensions.Options;

namespace MediaOrganizer.UI;

/// <summary>
/// Console-based user interface for the Media Organizer application.
/// Handles user interaction, menu navigation, and presentation of results.
/// </summary>
public class MediaOrganizerConsoleApp
{
    private readonly IConsoleIO _console;
    private readonly MediaOrganizerSettings _settings;
    private readonly MediaFileOrganizerFactory _organizerFactory;
    private readonly IDirectoryCleaner _directoryCleaner;
    private MediaFileOrganizer? _movieOrganizer;
    private MediaFileOrganizer? _tvShowOrganizer;

    protected MediaFileOrganizer MovieOrganizer =>
        _movieOrganizer ??= _organizerFactory.CreateMovieOrganizer();

    protected MediaFileOrganizer TvShowOrganizer =>
        _tvShowOrganizer ??= _organizerFactory.CreateTvShowOrganizer();

    public MediaOrganizerConsoleApp(IConsoleIO console,
                                    IOptions<MediaOrganizerSettings> settings,
                                    FileSystemValidator fileSystemValidator,
                                    MediaFileOrganizerFactory organizerFactory,
                                    IDirectoryCleaner directoryCleaner)
    {
        _console = console;
        _settings = settings.Value;
        _settings.SetValidator(fileSystemValidator);
        _organizerFactory = organizerFactory;
        _directoryCleaner = directoryCleaner;
    }

    public int Run()
    {
        _console.WriteLine("Media Organizer");
        _console.WriteLine("===============");

        if (!_settings.IsValid())
        {
            _console.WriteError("Configuration validation failed:");
            foreach (var error in _settings.GetValidationErrors())
            {
                _console.WriteLine($"   • {error}");
            }
            return 1;
        }

        _console.WriteSuccess("Configuration loaded successfully");

        _console.WriteLine($"TV Show Source Directory: {_settings.TvShowSourceDirectory}");
        if (!string.Equals(_settings.TvShowSourceDirectory, _settings.TvShowDestinationDirectory, StringComparison.OrdinalIgnoreCase))
        {
            _console.WriteLine($"TV Show Destination Directory: {_settings.TvShowDestinationDirectory}");
        }

        _console.WriteLine($"Movie Source Directory: {_settings.MovieSourceDirectory}");
        if (!string.Equals(_settings.MovieSourceDirectory, _settings.MovieDestinationDirectory, StringComparison.OrdinalIgnoreCase))
        {
            _console.WriteLine($"Movie Destination Directory: {_settings.MovieDestinationDirectory}");
        }

        _console.WriteLine($"Dry Run Mode: {(_settings.DryRun ? "Enabled" : "Disabled")}");
        _console.WriteLine("");

        return ShowMainMenu();
    }

    private int ShowMainMenu()
    {
        while (true)
        {
            _console.WriteLine("Main Menu");
            _console.WriteLine("---------");
            _console.WriteLine("1. TV Shows");
            _console.WriteLine("2. Movies");
            _console.WriteLine("3. Clean empty directories");
            _console.WriteLine("Q. Quit");
            _console.WriteLine("");
            _console.Write("Choose an option: ");

            var key = _console.ReadKey();
            _console.WriteLine(); // Add newline after key press

            switch (key.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    ShowTvShowMenu();
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    ShowMovieMenu();
                    break;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    CleanEmptyDirectories();
                    break;
                case ConsoleKey.Q:
                case ConsoleKey.Escape:
                    _console.WriteLine("Goodbye!");
                    return 0;
                default:
                    _console.WriteError("Invalid option. Please try again.");
                    break;
            }

            if (key.Key != ConsoleKey.Q)
            {
                _console.WriteLine("");
                _console.WriteLine("Press any key to continue...");
                _console.ReadKey(true);
                _console.WriteLine("");
            }
        }
    }

    private void ShowTvShowMenu()
    {
        ShowMediaMenu("TV Shows", TvShowOrganizer, _settings.TvShowSourceDirectory, 
            () => CleanMediaDirectories("TV Show", _settings.TvShowSourceDirectory, _settings.TvShowDestinationDirectory));
    }

    private void ShowMovieMenu()
    {
        ShowMediaMenu("Movies", MovieOrganizer, _settings.MovieSourceDirectory, 
            () => CleanMediaDirectories("Movie", _settings.MovieSourceDirectory, _settings.MovieDestinationDirectory));
    }

    private void ShowMediaMenu(string mediaType, MediaFileOrganizer organizer, string sourceDirectory, Action cleanDirectoriesAction)
    {
        while (true)
        {
            int count = organizer.RemainingCount;

            _console.WriteLine("");
            _console.WriteLine($"{mediaType} Menu");
            _console.WriteLine(new string('-', $"{mediaType} Menu".Length));
            _console.WriteLine($"1. List files ({count} found)");
            _console.WriteLine($"2. Organize files ({count} found)");
            _console.WriteLine("3. Clean directories");
            _console.WriteLine("B. Back to main menu");
            _console.WriteLine("");
            _console.Write("Choose an option: ");

            var key = _console.ReadKey();
            _console.WriteLine();

            switch (key.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    if (count == 0)
                    {
                        _console.WriteWarning($"No {mediaType} files found in the source directory.");
                    }
                    else
                    {
                        ListMediaFiles(organizer, sourceDirectory);
                    }
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    if (count == 0)
                    {
                        _console.WriteWarning($"No {mediaType} files found to organize.");
                    }
                    else
                    {
                        OrganizeMediaFiles(organizer);
                    }
                    break;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    cleanDirectoriesAction();
                    break;
                case ConsoleKey.B:
                case ConsoleKey.Escape:
                    return; // Back to main menu
                default:
                    _console.WriteError("Invalid option. Please try again.");
                    break;
            }

            if (key.Key != ConsoleKey.B && key.Key != ConsoleKey.Escape)
            {
                _console.WriteLine("");
                _console.WriteLine("Press any key to continue...");
                _console.ReadKey(true);
                _console.WriteLine("");
            }
        }
    }

    private void ListMediaFiles(MediaFileOrganizer organizer, string mediaSourceDirectory)
    {
        _console.WriteLine("");
        _console.WriteLine($"Media Source Directory: {mediaSourceDirectory}");

        if (!organizer.AllFiles.Any())
        {
            _console.WriteError("No video files found to organize.");
            return;
        }

        foreach (var file in organizer.AllFiles)
        {
            _console.WriteLine($"   {Path.GetRelativePath(mediaSourceDirectory, file.FullName)}");
        }
    }

    private void OrganizeMediaFiles(MediaFileOrganizer organizer)
    {
        _console.WriteLine("");
        _console.WriteLine("Interactive File Organization");
        _console.WriteLine("============================");
        
        if (organizer.RemainingCount == 0)
        {
            _console.WriteLine("No files to organize.");
            return;
        }
        
        _console.WriteLine("Controls:");
        _console.WriteLine("  ENTER - Organize current file");
        _console.WriteLine("  A     - Organize all remaining files");
        _console.WriteLine("  S     - Skip current file");
        _console.WriteLine("  B     - Back to main menu");
        _console.WriteLine("");
        
        ProcessFilesInteractively(organizer);
    }
    
    private void CleanMediaDirectories(string mediaType, string sourceDirectory, string destinationDirectory)
    {
        _console.WriteLine("");
        _console.WriteLine($"Clean {mediaType} Directories");
        _console.WriteLine(new string('=', $"Clean {mediaType} Directories".Length));
        
        _console.WriteLine($"Scanning {mediaType} directories:");
        _console.WriteLine($"  Source: {sourceDirectory}");
        _console.WriteLine($"  Destination: {destinationDirectory}");
        _console.WriteLine("");
        
        _directoryCleaner.CleanEmptyDirectories(sourceDirectory, destinationDirectory);
        
        _console.WriteSuccess($"{mediaType} directory cleanup completed");
    }

    private void ProcessFilesInteractively(MediaFileOrganizer organizer)
    {
        while (organizer.RemainingCount > 0)
        {
            var currentFile = organizer.PeekFile();
            
            _console.WriteLine($"Files remaining: {organizer.RemainingCount}");

            if (currentFile != null && currentFile.IsValid)
            {
                _console.WriteLine($"Next file: {Path.GetFileName(currentFile.OriginalFilePath)}");
                // TODO: Fix performance so feel free to generate the relative path many times like this
                _console.WriteLine($"Will organize as: {currentFile.GenerateRelativePath(_settings)}");
            }
            else
            {
                _console.WriteLine($"Next file: {Path.GetFileName(currentFile?.OriginalFilePath)}");
                _console.WriteError("Cannot parse this file - it will be skipped or failed");
            }
            
            _console.WriteLine("");
            _console.Write("Action (ENTER/A/S/B): ");
            
            var key = _console.ReadKey();
            _console.WriteLine();
            
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    var result = organizer.OrganizeFile();
                    if (result != null && result.IsValid)
                    {
                        _console.WriteSuccess($"Organized: {result}");
                    }
                    else
                    {
                        _console.WriteError("Failed to organize file");
                    }
                    break;
                case ConsoleKey.A:
                    _console.WriteLine("Organizing all remaining files...");
                    _ = organizer.OrganizeAllFiles();
                    break;
                case ConsoleKey.S:
                    organizer.SkipFile();
                    _console.WriteInformation("Skipped file");
                    break;
                case ConsoleKey.B:
                case ConsoleKey.Escape:
                    _console.WriteLine("Organization cancelled by user");
                    return;
                default:
                    _console.WriteError("Invalid key. Use ENTER, A, S, or ESC");
                    continue;
            }
            
            _console.WriteLine("");
        }
        
        var stats = organizer.Result;
        _console.WriteSuccess($"Organization complete: {stats.OrganizedCount} organized, {stats.SkippedCount} skipped, {stats.FailedCount} failed out of {stats.ProcessedCount} total files");

        if (_settings.AutoCleanupEmptyDirectories)
        {
            CleanEmptyDirectories();
        }
    }

    private void CleanEmptyDirectories()
    {
        _console.WriteLine("");
        _console.WriteLine("Clean Empty Directories");
        _console.WriteLine("=======================");
        
        _console.WriteLine($"Scanning directories in:");
        _console.WriteLine($"  TV Show Source: {_settings.TvShowSourceDirectory}");
        _console.WriteLine($"  TV Show Destination: {_settings.TvShowDestinationDirectory}");
        _console.WriteLine($"  Movie Source: {_settings.MovieSourceDirectory}");
        _console.WriteLine($"  Movie Destination: {_settings.MovieDestinationDirectory}");
        _console.WriteLine("");
        
        _directoryCleaner.CleanEmptyDirectories();
        
        _console.WriteSuccess("Empty directory cleanup completed");
    }
}
