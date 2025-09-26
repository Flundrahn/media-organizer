using System.IO.Abstractions;
using MediaOrganizer.Configuration;
using MediaOrganizer.Output;
using MediaOrganizer.Validations;
using Microsoft.Extensions.Options;

namespace MediaOrganizer.Services;
public class MediaOrganizerService
{
    private readonly IOutputWriter _output;
    private readonly MediaOrganizerSettings _settings;
    private readonly IMediaFileProvider _mediaFileProvider;

    public MediaOrganizerService(IOutputWriter output,
                                 IOptions<MediaOrganizerSettings> settings,
                                 IMediaFileProvider mediaFileProvider,
                                 FileSystemValidator fileSystemValidator)
    {
        _output = output;
        _settings = settings.Value;
        _settings.SetValidator(fileSystemValidator);
        _mediaFileProvider = mediaFileProvider;
    }

    public int Run()
    {
        _output.WriteLine("Media Organizer");
        _output.WriteLine("===============");

        if (!_settings.IsValid())
        {
            _output.WriteError("Configuration validation failed:");
            foreach (var error in _settings.GetValidationErrors())
            {
                _output.WriteLine($"   • {error}");
            }
            return 1;
        }

        _output.WriteSuccess("Configuration loaded successfully");
        _output.WriteLine($"📂 Source Directory: {_settings.SourceDirectory}");
        _output.WriteLine($"📂 Destination Directory: {_settings.DestinationDirectory}");
        _output.WriteLine($"🧪 Dry Run Mode: {(_settings.DryRun ? "Enabled" : "Disabled")}");
        _output.WriteLine("");

        return ShowMainMenu();
    }

    private int ShowMainMenu()
    {
        while (true)
        {
            var mediaFiles = _mediaFileProvider.GetMediaFiles(_settings.SourceDirectory);
            
            _output.WriteLine("Main Menu");
            _output.WriteLine("---------");
            _output.WriteLine($"1. List video files ({mediaFiles.Count()} found)");
            _output.WriteLine("2. Exit");
            _output.WriteLine("");
            _output.WriteLine("Choose an option (1-2):");

            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    ListMediaFiles(mediaFiles);
                    break;
                case "2":
                    _output.WriteLine("Goodbye!");
                    return 0;
                default:
                    _output.WriteError("Invalid option. Please choose 1 or 2.");
                    break;
            }

            if (input != "2")
            {
                _output.WriteLine("");
                _output.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
                _output.WriteLine("");
            }
        }
    }

    private void ListMediaFiles(IEnumerable<IFileInfo> mediaFiles)
    {
        _output.WriteLine("");
        _output.WriteLine($"📂 Source Directory: {_settings.SourceDirectory}");

        try
        {
            if (!mediaFiles.Any())
            {
                _output.WriteLine("No video files found.");
                return;
            }

            foreach (var file in mediaFiles)
            {
                _output.WriteLine($"   {file.FullName}");
            }
        }
        catch (Exception ex)
        {
            _output.WriteError($"Error listing files: {ex.Message}");
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