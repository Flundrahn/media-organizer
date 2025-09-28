using System.IO.Abstractions;
using MediaOrganizer.Configuration;
using MediaOrganizer.IO;
using MediaOrganizer.Validations;
using Microsoft.Extensions.Options;

namespace MediaOrganizer.Services;
public class MediaOrganizerService
{
    private readonly IConsoleIO _console;
    private readonly MediaOrganizerSettings _settings;
    private readonly IMediaFileProvider _mediaFileProvider;
    private readonly MediaFileOrganizer _organizer;

    public MediaOrganizerService(IConsoleIO console,
                                 IOptions<MediaOrganizerSettings> settings,
                                 IMediaFileProvider mediaFileProvider,
                                 FileSystemValidator fileSystemValidator,
                                 MediaFileOrganizer organizer)
    {
        _console = console;
        _settings = settings.Value;
        _settings.SetValidator(fileSystemValidator);
        _mediaFileProvider = mediaFileProvider;
        _organizer = organizer;
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
        _console.WriteLine($"Source Directory: {_settings.SourceDirectory}");
        _console.WriteLine($"Destination Directory: {_settings.DestinationDirectory}");
        _console.WriteLine($"Dry Run Mode: {(_settings.DryRun ? "Enabled" : "Disabled")}");
        _console.WriteLine("");

        return ShowMainMenu();
    }

    private int ShowMainMenu()
    {
        while (true)
        {
            var mediaFiles = _mediaFileProvider.GetMediaFiles(_settings.SourceDirectory);
            int count = mediaFiles.Count();
            
            _console.WriteLine("Main Menu");
            _console.WriteLine("---------");
            _console.WriteLine($"1. List video files ({count} found)");
            _console.WriteLine($"2. Organize files ({count} found)");
            _console.WriteLine("Q. Quit");
            _console.WriteLine("");
            _console.Write("Choose an option: ");

            var key = _console.ReadKey();
            _console.WriteLine(); // Add newline after key press

            switch (char.ToUpper(key.KeyChar))
            {
                case '1':
                    ListMediaFiles(mediaFiles);
                    break;
                case '2':
                    OrganizeMediaFiles(mediaFiles);
                    break;
                case 'Q':
                    _console.WriteLine("Goodbye!");
                    return 0;
                default:
                    _console.WriteError("Invalid option. Please try again.");
                    break;
            }

            if (char.ToUpper(key.KeyChar) != 'Q')
            {
                _console.WriteLine("");
                _console.WriteLine("Press any key to continue...");
                _console.ReadKey(true);
                _console.WriteLine("");
            }
        }
    }

    private void ListMediaFiles(IEnumerable<IFileInfo> mediaFiles)
    {
        _console.WriteLine("");
        _console.WriteLine($"Source Directory: {_settings.SourceDirectory}");

        try
        {
            // TODO: validate this and show feedback in main menu instead using count
            // if (!mediaFiles.Any())
            // {
            //     _console.WriteLine("No video files found.");
            //     return;
            // }

            foreach (var file in mediaFiles)
            {
                _console.WriteLine($"   {Path.GetRelativePath(_settings.SourceDirectory, file.FullName)}");
            }
        }
        catch (Exception ex)
        {
            _console.WriteError($"Error listing files: {ex.Message}");
        }
    }

    private void OrganizeMediaFiles(IEnumerable<IFileInfo> mediaFiles)
    {
        _console.WriteLine("");
        _console.WriteLine("Interactive File Organization");
        _console.WriteLine("============================");
        
        try
        {
            _organizer.Initialize(mediaFiles);
            
            if (_organizer.RemainingCount == 0)
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
        catch (Exception ex)
        {
            _console.WriteError($"Error during file organization: {ex.Message}");
        }
    }

    private void ProcessFilesInteractively()
    {
        while (_organizer.RemainingCount > 0)
        {
            var currentFile = _organizer.PeekFile();
            
            _console.WriteLine($"Files remaining: {_organizer.RemainingCount}");
            
            if (currentFile != null && currentFile.IsValid)
            {
                _console.WriteLine($"Next file: {currentFile.OriginalFile.Name}");
                _console.WriteLine($"Will organize as: {currentFile.TvShowName} | S{currentFile.Season:D2}E{currentFile.Episode:D2} | {currentFile.Title}");
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
                    var result = _organizer.OrganizeFile();
                    if (result != null && result.IsValid)
                    {
                        _console.WriteSuccess($"Organized: {result.TvShowName} - S{result.Season:D2}E{result.Episode:D2}");
                    }
                    else
                    {
                        _console.WriteError("Failed to organize file");
                    }
                    break;
                case ConsoleKey.A:
                    _console.WriteLine("Organizing all remaining files...");
                    var finalResult = _organizer.OrganizeAllFiles();
                    _console.WriteSuccess($"Batch complete: {finalResult.OrganizedCount} organized, {finalResult.SkippedCount} skipped, {finalResult.FailedCount} failed");
                    return;
                case ConsoleKey.S:
                    _organizer.SkipFile();
                    _console.WriteLine("Skipped file");
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
        
        var stats = _organizer.Result;
        _console.WriteSuccess($"Organization complete: {stats.OrganizedCount} organized, {stats.SkippedCount} skipped, {stats.FailedCount} failed out of {stats.ProcessedCount} total files");
    }

    private string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1}{suffixes[counter]}";
    }
}
