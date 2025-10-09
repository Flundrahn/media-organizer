# Media Organizer - TODO

## Done ✅
- [x] Configuration loading from appsettings.json
- [x] File system validation with proper error reporting
- [x] Interactive main menu with file count display
- [x] Video file discovery with extension filtering (.mp4, .avi, .mkv, .mov, .wmv)
- [x] Subdirectory inclusion/exclusion support
- [x] File listing with sizes and full paths
- [x] Options pattern for dependency injection
- [x] Comprehensive test coverage (23/23 tests passing)
- [x] IFileInfo integration for enhanced file metadata
- [x] File size formatting (B, KB, MB, GB, TB)
- [x] Clean architecture with proper service separation
- [x] Console output abstraction for testability
- [x] Tv show file parser.
- [x] File organization/moving functionality
- [x] Basic logging system
- [x] Different file renaming patterns
- [x] Fix bug with unparse-able file spaced with spaces instead of dots
- [x] Use env dependent path separation character
- [x] Fix bugs with certain unparse-able files
- [x] Support using full paths for organizing tv show files to search within existing show and episode directories
- [x] Interactive file organization with options to organize one file at a time, skip files, or organize all remaining files
- [x] Add support to configure allowed video file extensions
- [x] Add support to clean up empty directories after moving files
- [x] Add some integration tests
- [x] Refactor before continuing movie feature
    - factory methods to create specific organizer
    - rename parser interface to make generic
    - update config of file provider
    - inject configured file provider into organizer in factory method: ended up not doing. Nice and decoupled to inject FileInfos into ctor of organizer.

## Doing
- [ ] Movie feature

## TODO: Movie Feature Plan 🎬

### Overview
Add support for organizing movie files alongside TV shows by creating a common media file abstraction. Movies and TV shows will be distinguished by directory context rather than file parsing.

### Architecture Changes

#### Phase 1: Foundation ✨ ✅ COMPLETE
- [x] Create `IMediaFile` interface with common properties and methods
- [x] Refactor `TvShowEpisode` to implement `IMediaFile` 
- [x] Update `MediaFileOrganizer` to work with `IMediaFile` instead of `TvShowEpisode`
- [x] Ensure all existing tests pass with interface changes

#### Phase 2: Movie Support 🎭
- [x] Create `Movie` class implementing `IMediaFile`
  - Properties: `Title`, `Year`, `Quality` (1080p, 4K, etc.)
  - Support placeholders: `{Title}`, `{Year}`, `{Quality}`
- [x] Create `MovieParser` class implementing `IMediaFileParser`
  - Parse patterns like: `Movie.Title.2023.1080p.BluRay.x264.mkv`
  - Parse patterns like: `The Matrix (1999) [1080p].mp4`
- [x] Add `MoviePathTemplate` to `MediaOrganizerSettings`
- [x] Add specific tv and movie media folders to `MediaOrganizerSettings`
  - Example: `"Movies/{Title} ({Year})"` → `"Movies/The Matrix (1999).mkv"`

#### Phase 3: Directory Context 📁
- [ ] Add media type detection based on source directory or user selection
- [ ] Update console UI to allow user to choose between TV shows and movies
- [ ] MediaFileOrganizer receives media type as parameter, not composite parsing
- [ ] Add separate menu options: "Organize TV Shows" and "Organize Movies"

#### Phase 4: Integration & Testing 🧪
- [ ] Update DI registration for new parsers
- [ ] Add movie-specific unit tests
- [ ] Add movie integration tests using `TempMediaTestEnvironment`
- [ ] Update configuration validation for movie path templates
- [ ] Add logging for media type detection

#### Phase 5: Polish 💎
- [ ] Update console UI to show media type in file listings
- [ ] Add validation for movie path template placeholders
- [ ] Improve error handling for unknown media types
- [ ] Update documentation and examples

### Example Movie Patterns
```regex
// High confidence patterns
Movie\.Title\.(\d{4})\..*\.(mkv|mp4|avi)
The\.Movie\.Title\.(\d{4})\.(\d{4}p)\..*

// Medium confidence patterns  
([A-Za-z\s]+)\s+\((\d{4})\).*
([A-Za-z\s]+)\.(\d{4})\..*

// Fallback patterns (no series indicators)
^(?!.*[Ss]\d{2}[Ee]\d{2})([A-Za-z\s]+).*\.(mkv|mp4|avi)$
```

## TODO - Core Features

- [ ] File organization/moving: Strategy pattern for different organization methods
- [ ] Add readme
- [ ] Add smooth build and publish setup, maybe checkout GitHub actions and release.
- [ ] Possibly refactor MediaOrganizerService into Program.cs, just extract parts that build app and so on, make clear which parts are logic and which parts are UI.
- [ ] Cleanup double validation TvShow and movie model
- [ ] Extract cleanup module from MediaFileOrganizer, it should be independend module.
- [ ] Use microsoft package for console options that can display usage and so on.
- [ ] Settings validate one property at a time in own method
- [ ] Add quality to TvShowEpisode and parser

## Nice to Have 🌟

### Core Features
- [ ] Generate regex in compile time for performance
- [ ] Batch operations with progress tracking
- [ ] Better error handling and user feedback
- [ ] Undo operations for file moves
- [ ] Fetch metedata from online databases (e.g., TheMovieDB, IMDb)
- [ ] Duplicate file detection and handling

### Enhanced User Experience
- [ ] File preview/details view with metadata
- [ ] Filtering options (by size, date, extension)
- [ ] Recent operations history
- [ ] Default to destination folders same as source - unless specified.

### Advanced Features
- [ ] Metadata extraction (resolution, duration, codec info)
- [ ] Performance optimizations for large directories
- [ ] Async operations for better responsiveness
- [ ] Plugin/extension system for custom processors

### Configuration & Automation
- [ ] Export/import settings profiles
- [ ] Command-line arguments support for automation
- [ ] Watch folders for automatic processing
- [ ] Scheduled operations
- [ ] appSettings.Local.json support for local overrides, (check in and deploy appSettings.Development?)
- [ ] Configuration menu for runtime settings changes => write to appSettings.Local.json

### Platform & Integration
- [ ] GUI version (WPF/MAUI)
- [ ] Web interface for remote management
- [ ] Portable/standalone version

## Known Issues 🐛
- [ ] Console input issues when input is redirected (app hangs with piped input or automation)
- [ ] Large directory scanning may be slow
- [ ] No graceful handling of locked files

## Development Notes 📝
- Keep backward compatibility with existing configuration files
- Maintain comprehensive test coverage for new features
- Follow .NET best practices and patterns
- Consider performance impact of new features
- Prioritize core functionality over nice-to-have features


## Won't Do
- [ ] Support for additional media formats (images, audio)