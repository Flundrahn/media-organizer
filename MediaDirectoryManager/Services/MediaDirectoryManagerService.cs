using MediaOrganizer.Configuration;
using MediaOrganizer.Output;

namespace MediaOrganizer.Services;

public class MediaOrganizerService
{
    private readonly IOutputWriter _output;
    private readonly MediaOrganizerSettings _settings;

    public MediaOrganizerService(IOutputWriter output, MediaOrganizerSettings settings)
    {
        _output = output;
        _settings = settings;
    }

    public int Run()
    {
        _output.WriteLine("Media Organizer");
        _output.WriteLine("======================");

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

        return 0;
    }
}