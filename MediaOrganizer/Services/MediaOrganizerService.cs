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
            
            _console.WriteLine("Main Menu");
            _console.WriteLine("---------");
            _console.WriteLine($"1. List video files ({mediaFiles.Count()} found)");
            _console.WriteLine($"2. Organize files ({mediaFiles.Count()} found)");
            _console.WriteLine("3. Exit");
            _console.WriteLine("");
            _console.WriteLine("Choose an option (1-3):");

            var input = _console.ReadLine();

            switch (input?.Trim())
            {
                case "1":
                    ListMediaFiles(mediaFiles);
                    break;
                case "2":
                    OrganizeMediaFiles(mediaFiles);
                    break;
                case "3":
                    _console.WriteLine("Goodbye!");
                    return 0;
                default:
                    _console.WriteError("Invalid option. Please choose 1, 2, or 3.");
                    break;
            }

            if (input != "3")
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
            if (!mediaFiles.Any())
            {
                _console.WriteLine("No video files found.");
                return;
            }

            foreach (var file in mediaFiles)
            {
                _console.WriteLine($"   {file.FullName}");
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
        _console.WriteLine("Starting file organization...");
        
        try
        {
            var result = _organizer.OrganizeFiles(mediaFiles);
            
            if (result)
            {
                _console.WriteSuccess("File organization completed successfully!");
            }
            else
            {
                _console.WriteError("File organization completed with some issues. Check logs for details.");
            }
        }
        catch (Exception ex)
        {
            _console.WriteError($"Error during file organization: {ex.Message}");
        }
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
