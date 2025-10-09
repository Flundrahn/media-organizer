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
    private MediaFileOrganizer? _organizer;

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

        _organizer = _organizerFactory.CreateTvShowOrganizer();

        return ShowMainMenu();
    }

    protected MediaFileOrganizer Organizer =>
        _organizer ?? throw new InvalidOperationException("Organizer not initialized. Call Run() first.");

    // TODO: List tv and movie separately
    // TODO: Organize tv and movie separately
    // TODO: Clean tv and movie separately
    // NOTE: 'All' options? Leave until realize actually useful
    private int ShowMainMenu()
    {
        while (true)
        {
            int count = Organizer.RemainingCount;

            _console.WriteLine("Main Menu");
            _console.WriteLine("---------");
            _console.WriteLine($"1. List video files ({count} found)");
            _console.WriteLine($"2. Organize files ({count} found)");
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
                    if (count == 0)
                    {
                        _console.WriteWarning("No video files found in the source directory.");
                    }
                    else
                    {
                        ListMediaFiles();
                    }
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    if (count == 0)
                    {
                        _console.WriteWarning("No video files found to organize.");
                    }
                    else
                    {
                        OrganizeMediaFiles();
                    }
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

    private void ListMediaFiles()
    {
        _console.WriteLine("");
        _console.WriteLine($"TV Show Source Directory: {_settings.TvShowSourceDirectory}");
        _console.WriteLine($"Movie Source Directory: {_settings.MovieSourceDirectory}");

        if (!Organizer.AllFiles.Any())
        {
            _console.WriteError("No video files found to organize.");
            return;
        }

        foreach (var file in Organizer.AllFiles)
        {
            _console.WriteLine($"   {Path.GetRelativePath(_settings.SourceDirectory, file.FullName)}");
        }
    }

    private void OrganizeMediaFiles()
    {
        _console.WriteLine("");
        _console.WriteLine("Interactive File Organization");
        _console.WriteLine("============================");
        
        if (Organizer.RemainingCount == 0)
        {
            _console.WriteLine("No files to organize.");
            return;
        }
        
        _console.WriteLine("Controls:");
        _console.WriteLine("  ENTER - Organize current file");
        _console.WriteLine("  A     - Organize all remaining files");
        _console.WriteLine("  S     - Skip current file");
        _console.WriteLine("  ESC   - Exit organization");
        _console.WriteLine("");
        
        ProcessFilesInteractively();
    }

    private void ProcessFilesInteractively()
    {
        while (Organizer.RemainingCount > 0)
        {
            var currentFile = Organizer.PeekFile();
            
            _console.WriteLine($"Files remaining: {Organizer.RemainingCount}");
            
            if (currentFile != null && currentFile.IsValid)
            {
                _console.WriteLine($"Next file: {currentFile.OriginalFile.Name}");
                _console.WriteLine($"Will organize as: {currentFile}");
            }
            else
            {
                _console.WriteLine($"Next file: {currentFile?.OriginalFile.Name}");
                _console.WriteError("Cannot parse this file - it will be skipped or failed");
            }
            
            _console.WriteLine("");
            _console.Write("Action (ENTER/A/S/ESC): ");
            
            var key = _console.ReadKey();
            _console.WriteLine();
            
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    var result = Organizer.OrganizeFile();
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
                    var finalResult = Organizer.OrganizeAllFiles();
                    _console.WriteSuccess($"Batch complete: {finalResult.OrganizedCount} organized, {finalResult.SkippedCount} skipped, {finalResult.FailedCount} failed");
                    return;
                case ConsoleKey.S:
                    Organizer.SkipFile();
                    _console.WriteInformation("Skipped file");
                    break;
                case ConsoleKey.Escape:
                    _console.WriteLine("Organization cancelled by user");
                    return;
                default:
                    _console.WriteError("Invalid key. Use ENTER, A, S, or ESC");
                    continue;
            }
            
            _console.WriteLine("");
        }
        
        var stats = Organizer.Result;
        _console.WriteSuccess($"Organization complete: {stats.OrganizedCount} organized, {stats.SkippedCount} skipped, {stats.FailedCount} failed out of {stats.ProcessedCount} total files");
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