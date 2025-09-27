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

## Doing
- [ ] Fix bugs with certain unparse-able files

## TODO - Core Features

- [ ] Organize one file at a time or skip or all
- [ ] File organization/moving: Strategy pattern for different organization methods
- [ ] Support using full paths for organizing tv show files to search within existing show and episode directories

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