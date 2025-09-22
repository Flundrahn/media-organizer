using MediaOrganizer.Configuration;
using MediaOrganizer.Output;
using Microsoft.Extensions.Options;

namespace MediaOrganizer.Services;

public class MediaOrganizerService
{
    private readonly IOutputWriter _output;
    private readonly MediaOrganizerSettings _settings;
    private readonly IMediaFileProvider _mediaFileProvider;

    public MediaOrganizerService(IOutputWriter output, IOptions<MediaOrganizerSettings> settings, IMediaFileProvider mediaFileProvider)
    {
        _output = output;
        _settings = settings.Value;
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
            var fileCount = GetFileCount();
            
            _output.WriteLine("Main Menu");
            _output.WriteLine("---------");
            _output.WriteLine($"1. List video files ({fileCount} found)");
            _output.WriteLine("2. Exit");
            _output.WriteLine("");
            _output.WriteLine("Choose an option (1-2):");

            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    ListFiles();
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

    private void ListFiles()
    {
        _output.WriteLine("");
        _output.WriteLine($"📂 Source Directory: {_settings.SourceDirectory}");

        try
        {
            var files = _mediaFileProvider.GetMediaFiles(_settings.SourceDirectory);
            
            if (files.Any())
            {
                _output.WriteSuccess($"Found {files.Count()} video file(s):");
                foreach (var file in files)
                {
                    _output.WriteLine($"   • {file}");
                }
            }
            else
            {
                _output.WriteLine("No video files found.");
            }
        }
        catch (Exception ex)
        {
            _output.WriteError($"Error listing files: {ex.Message}");
        }
    }

    private int GetFileCount()
    {
        try
        {
            var files = _mediaFileProvider.GetMediaFiles(_settings.SourceDirectory);
            return files.Count();
        }
        catch
        {
            return 0;
        }
    }
}