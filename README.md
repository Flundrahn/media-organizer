# Media Organizer

A .NET console application for organizing and managing media files (TV shows and movies) with intelligent parsing and metadata enrichment capabilities.

## Features

- **TV Show Organization**: Parse episode information from filenames and organize into structured directories
- **Movie Organization**: Identify and organize movie files with metadata extraction
- **Flexible Path Templates**: Customize how your media files are organized by using templates in appsettings.json 
- **Empty Directory Cleanup**: Automatically remove empty directories after file operations

## Getting Started

### Prerequisites

- .NET 9 SDK

### Installation

1. Clone the repository:
```bash
git clone https://github.com/Flundrahn/media-directory-manager.git
cd media-organizer
```

2. Build the solution:
```bash
dotnet build
```

3. Configure your settings in `appsettings.json`, or use override settings in a development environment file like `appsettings.Development.json` according to .NET conventions, default environment name is "Production".

### Usage

Run the application:
```bash
dotnet run --project MediaOrganizer
```

Navigate through the interactive menu to:
- Discover media files in your source directory
- Preview parsed information
- Organize files into destination directories
- Clean up empty directories

## Project Structure

```
media-organizer/
├── MediaOrganizer/              # Main console application
├── MediaOrganizer.Tests/        # Unit tests
├── MediaOrganizer.IntegrationTests/  # Integration tests
└── MediaOrganizer.Benchmarks/   # Investigative benchmarks
```

## Design Decisions

### Dependency Injection and Factories

The main brains of the app is the class MediaFileOrganizer, for this class I wanted to find design patterns that would allow me to:
- be able to create different types of organizers (TV show vs movie vs any future media type)
- while avoiding complicated inheritence => composition over inheritance, use the composition to reuse common logic.
- be able to create these differently configured organizers on demand, it would be annoying having to instantiate each one pre-emptively before user has selected what to do.
- while avoiding service locator anti-pattern => all class dependencies should be clearly visible in constructors

After some thinking I found what seems obvious in retrospect, that I could use a `MediaFileOrganizerFactory` to hold the knowledge of how to configure different `MediaFileOrganizers`, and if I just gave the factory itself delegate factory methods to on-demand instantiate all the types required to create the different `MediaFileOrganizers`.
For current functionality this is for sure over-engineering, but it seems a great pattern to have in the toolbox. 

### Options pattern

The app uses the Options pattern for configuration, using a strongly typed `MediaOrganizerSettings` class that maps to the `appsettings.json` configuration, while also holding validation of the settings making it easy to return descriptive errors, creating a living documentation of the app.
If the project would be larger, with more settings classes, I would split the validation into its own class, this is fine for now. It's a simple thing but so clean and useful I do it in most new projects.

### Template for file organization

The application uses customizable path templates defined in `appsettings.json` to organize your media files. These templates support placeholders, see the ValidPlaceholders properties on the Movie and TvShowEpisode classes.
This should allow users to easily customize the structure they want for their media library, while also keeping it extensible for future developement.

**Example Configuration:**
````````json
{
  "MediaOrganizerSettings": {
    "FileOrganization": {
      "TVShowTemplate": "{TvShowName}/Season {Season:D2}/{TvShowName} - S{Season:D2}E{Episode:D2} - {Title}.mkv",
      "MovieTemplate": "{Title} ({Year})/{Title} {Quality}.mkv"
    }
  }
}
````````

This would organize files like:
- TV: `Breaking Bad/Season 01/Breaking Bad - S01E01 - Pilot.mkv`
- Movie: `Inception (2010)/Inception 1080p.mkv`


### ConsoleIO

I added this to the DI-container, with the idea to abstract away the console input and output, making for better testability. It is useful in the integration tests of the actual console app, however I don't know if it was fully worth it, there may be existing standard ways of testing a .NET console app.
I will continue using it in this project for now as a learning experience, if it proves easy to work with I'll keep it, else if it proves cumbersome I'll remove it as I encounter the cumber.

## Known issues

Current app only extracts info from the filename, depending on what path template a user chooses this means less information may be available after organizing. It is currently possible taking out information, but not putting it back in.

## Future Development

For future development I have work in progress to add

- metadata extraction from the actual media files
- integration with TMDB API to enrich metadata
- refactoring so that a MediaFileOrganizer can use a pipeline or "enrichers", e.g. FileMetaDataMovieEnricher, TmdbApiMovieEnricher, KodiNfoMovieEnricher etc. to together create a more stable way of building up metadata.
- possibly add a SqLite DB to store metadata for the previously organized files, this would allow a user to put minimal information in the filenames and paths, while later being able to add it back.


## Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test MediaOrganizer.IntegrationTests
```

### Running Benchmarks

```bash
dotnet run --project MediaOrganizer.Benchmarks --configuration Release
```

This will display a menu and information on how to run the different benchmarks.

## Development Process

Approximately half of this project was built using GitHub Copilot (primarily Claude) in agent mode, and voice input in VS Code, because I had some painful ulnar nerve issues at the time. This constraint led to interesting insights:

- Voice input isn't bad for high level instructions 
- AI agent mode is fun
- Unit tests are vital for developing with AI
- Bothering with setting up a few integration tests is worth it, if you know you will continue to work the project for a while
- Abstractions like IFileSystem along with good use of DI is really nice and useful
- Having benchmarking experience is really useful for practical learning about performance, but it is a bit of a time investment, optimizing prematurely is a timewaste, unless you're doing it for learning ^^.
