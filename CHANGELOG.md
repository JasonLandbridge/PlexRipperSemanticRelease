## [0.3.0] - 2020-12-22
### Added
- Added media icons to the download confirmation dialog.
- Tv shows now show their media size in the MediaTableView.

### Changed
- Cleaned-up the default-layout code in the front-end.
- Moved the files related to path retrieval of the host system to the FileSystem project.

### Fixed
- Fixed the setup page not being scroll-able when viewing with limited viewport height.
- Fixed when going to a music library that nothing would happen, now a page will show that it is currently not supported.
- Fixed the UI for the path settings and DirectoryBrowser

## [0.2.0] - 2020-12-22
### Added
 - Added CHANGELOG.md file
 - Added PlexRipper version to the front-end in the home button.
 - Added the FileSize.vue component and moved the PrettyBytes filter there.
 - The total media size of a Plex library is now shown in the media bar on the library page.

### Changed
- Changed the dockerFile path for tvShows from "/series" to "/tvshows"
- Movies which are downloaded now include the year of release in their folder name.
- Refactored the config pipeline on front-end startup to work in both development and production.

### Fixed
- Fixed the CORS issue for SignalR due to missing "AllowCredentials".
- Fixed a bug in the refreshing of PlexLibraries which have the TvShow type.
  It would not wait for the result of all media, leading to wrong return values

### Removed
- Removed the CorsMiddleware fix in the back-end as it was not needed anymore


## [0.1.1] - 2020-12-18
### Changed
 - Updated packages front-end packages.
 - Setup build pipeline in Github Actions and in DockerHub.

## [0.1.0] - 2020-12-18
 - Initial release of PlexRipper