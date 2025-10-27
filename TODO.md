# Media Organizer - TODO

## 🔥 Currently Working On

Make a commit with a simple message when done with each step

FEATURE: Enrich with metadata from The Movie Database (TMDB) API and use that in new parsers for TvShowEpisodes and Movies, keeping the old ones for now.
    - [x] Use TMDbLib client directly for TMDB API
        - [x] Register TMDbLib client with DI.
        - [x] Add integration test for connectivity.
        - [x] Add validation of API config with tests.
    - [ ] TvShowEpisodeParser2. Do not make any changes in MediaOrganizerFactory or any user facing implementation, for now we are only adding the new classes and functionality and testing it.
        - [x] Add wrapper class for TMDB api client
        - [x] Add domain logic class that uses wrapper, it should use result pattern to elegantly handle any potential http errors, it should provide good logging of respones. Add unit tests for this mocking the response of the wrapper for happy and sad cases.
        - [x] Add integration test to ensure that TMDB API client works, if we have show name, season and episode number we should be able to get year of show and episode title
        - [x] Define interface for enrichment.
        - [x] Add class that uses api client to enrich TvShowEpisode objects with info from TMDB api
        - [ ] Add class that enriches tv show objects with info from file path. Copy and use same regex from old TvShowEpisodeParser but we will now only show name, season number, episode number.
        - [ ] Add class TvShowEpisodeParser2, will be able to use multiple enrichers in order to add and improve info of a TvShowEpisode object
    - [ ] Use TvShowEpisodeParser2 in media organizer classes instead of old parser.

## Priority Features 
Do these first since will affect and help how solve the cricital issues below

- [ ] Add DB SQLite and ORM
    - Add settings for DB and ORM (create if not exists, connection string, etc)
    - Save Movies TvShows and either Episodes or Files in DB

- [ ] Add video file metadata extractor using MediaInfo approach similar to in our benchmark
    - Refactor to use extractor get quality from files

## 🚨 Critical Issues
- [ ] **Problem: does not move auxiliary files along with main video for movie or tv show** - Core functionality gap - Possibly this would be easier if we store actual show and movie metadata in DB or corresponding.
- [ ] **Problem: does not rename subtitle files along with corresponding video file** - Related to above, possibly wait if will add DB anyway.
- [ ] **Problem: does not use the full path when organizing media** if do not have say title, season or episode in file name will not find it - Core functionality issue

## 🔧 Core Improvements
- [ ] **File organization/moving: Strategy pattern for different organization methods** - Architecture improvement
- [ ] **DRY double validation TvShow and movie model** - Code quality
- [ ] **Feature: cleanup jpg and nfo (and txt?) files** - Extends existing cleanup feature

## 📚 Documentation & DevEx
- [ ] **Add readme** - Essential for project usability
- [ ] **Add smooth build and publish setup, maybe checkout GitHub actions and release** - DevEx
- [ ] **Use microsoft package for console options that can display usage and so on** - Better CLI UX

## 🏗️ Refactoring & Architecture
- [ ] **Possibly refactor MediaOrganizerService into Program.cs** - Separate UI from logic
- [ ] **Add quality to TvShowEpisode and parser** - Feature enhancement

## Nice to Have 🌟

### Core Features
- [ ] Batch operations with progress tracking
- [ ] Better error handling and user feedback
- [ ] Undo operations for file moves
- [ ] Fetch metedata from online databases (e.g., TheMovieDB, IMDb)
- [ ] Duplicate file detection and handling
- [ ] Rate limiting for API calls, TMDB docs `50 requests per second range. This limit could change at any time so be respectful of the service we have built and respect the 429` handling of 429 responses.

### Enhanced User Experience
- [ ] File preview/details view with metadata
- [ ] Filtering options (by size, date, extension)
- [ ] Recent operations history
- [ ] Default to destination folders same as source - unless specified

### Advanced Features
- [ ] Metadata extraction (resolution, duration, codec info)
- [ ] Performance optimizations for large directories
- [ ] Async operations for better responsiveness
- [ ] Plugin/extension system for custom processors
- [ ] Add DB to keep state of media directories. Can validate files are where last was. Can have events NewTvShowAdded to decouple handler. Then use API to fetch metadata including episode names, that way can keep all info without storing it in names of shows as now.

### Optimizing C#
- [ ] Figure out when useful to do ConfigureAwait(false) or not
- [ ] Possibly remove usage of TMDbLib and create own client
- [ ] Add caching of Tv Show searches for TMDB API
    TODO: Possibly want to split up the public methods that will call the API to enable batching
    when use DB should check if exists in db first, or if use some local cache.
    Would be cool in that case to put cache in the impl of httpclient
    let's get functionality working first, then think about changes for batching

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

## Won't Do
- [ ] Support for additional media formats (images, audio)

## Done 
- [x] Configuration loading from appsettings.json
- [x] File system validation with proper error reporting
- [x] Interactive main menu with file count display
- [x] Video file discovery with extension filtering (.mp4, .avi, .mkv, .mov, .wmv)
- [x] Subdirectory inclusion/exclusion support
- [x] File listing with sizes and full paths
- [x] Options pattern for dependency injection
- [x] Comprehensive test coverage (268/268 tests passing)
- [x] IFileInfo integration for enhanced file metadata
- [x] File size formatting (B, KB, MB, GB, TB)
- [x] Clean architecture with proper service separation
- [x] Console output abstraction for testability
- [x] Tv show file parser
- [x] File organization/moving functionality
- [x] Basic logging system
- [x] Different file renaming patterns
- [x] Fix bug with unparse-able file spaced with spaces instead of dots
- [x] Use env dependent path separation character
- [x] Fix bugs with certain unparse-able files
- [x] Interactive file organization with options to organize one file at a time, skip files, or organize all remaining files
- [x] Add support to configure allowed video file extensions
- [x] Add support to clean up empty directories after moving files
- [x] Add some integration tests
- [x] Refactor before continuing movie feature
- [x] Movie feature
- [x] Extract cleanup module from MediaFileOrganizer, it should be independent module
- [x] Complete cleanup directory console UI to able manually cleanup either tv show or movie directories
- [x] **Generate regex in compile time for performance** - Implemented RegexOptions.Compiled for better performance
- [x] **Settings validate one property at a time in own method** - Code quality

# WORKING NOTES: String Deduplicator

Started create string info deduplicator with preprocess, alt Lempel–Ziv string deduplicator, did work fully, will remove duplicate words if both in show name and episode title e.g. "the".

DEDUPLICATOR - what about taking the full parts, such as folder and file names, and deduplicating those full strings?
- then take that final result and run through regexes
- keep the first or last? The last would fit better with existing patterns.
- but they may contain duplication but not be exact matches in the file name.
- GOAL: 
    - movie name
    - tv show name
    - season number
    - episode number

possibly take multiple possible movie name strings,
search for match in API
- folder movie name
- file movie name

- don't solve difficult general problem. If have something that works for all current files, is good enough.
 all files have movie name in both folder and file name
- all tv show episodes have show name in both folder and file name, and season and episode in file name
- this means current patterns using only files will work for me 