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
- [x] Movie feature
- [x] Extract cleanup module from MediaFileOrganizer, it should be independend module.
- [x] Complete cleanup directory console UI to able manually cleanup either tv show or movie directories.

## Doing

## TODO - Core Features

- [ ] File organization/moving: Strategy pattern for different organization methods
- [ ] Add readme
- [ ] Add smooth build and publish setup, maybe checkout GitHub actions and release.
- [ ] Possibly refactor MediaOrganizerService into Program.cs, just extract parts that build app and so on, make clear which parts are logic and which parts are UI.
- [ ] Cleanup double validation TvShow and movie model
- [ ] Use microsoft package for console options that can display usage and so on.
- [ ] Settings validate one property at a time in own method
- [ ] Add quality to TvShowEpisode and parser
- [ ] Problem: does not use the full path when organizing media, if put year in folder cannot put it back in file.
- [ ] Problem: does not move auxiliary files along with main video for movie or tv show.
- [ ] Problem: does not rename subtitle files along with corresponding video file
- [ ] Bug: consumes character when pressing esc to go back
- [ ] Feature: cleanup jpg and nfo (and txt?)

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
- Maintain comprehensive test coverage for new features
- Follow .NET best practices and patterns
- Consider performance impact of new features
- Prioritize core functionality over nice-to-have features

## Won't Do
- [ ] Support for additional media formats (images, audio)