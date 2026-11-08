# Changelog

This Changelog's format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added

- Exclude `DoNotShip` folders from the ZIP files

### Fixed

- Escape spaces in file paths
- ZIP files made on Windows use `\` as path separator opposed to the standard `/` (#3)

### Changed

- Move non-Windows zip builder into its own class

## [2.0.0] - 2026-07-18

### Added

- ZIP creation of *any* target platform builds

### Changed

- Package name is now "BuildZipper"
- ZIP settings on a per-target-platform

## [1.0.1] - 2024-11-26

### Fixed

- Build name couldn't contain spaces (#1)

## [1.0.0] - 2024-11-15

### Added

- Automatic build zipping of MacOS builds
	- On Windows machines, use WSL or manual ZIP creation methods
	- On Linux and MacOS, use native ZIP creation methods
- Project-wide ZIP settings

[Unreleased]: https://github.com/u-plus-one/BuildZipper/compare/2.0.0...HEAD
[2.0.0]: https://github.com/u-plus-one/BuildZipper/compare/1.0.1...2.0.0
[1.0.1]: https://github.com/u-plus-one/BuildZipper/compare/1.0.0...1.0.1
[1.0.0]: https://github.com/u-plus-one/BuildZipper/releases/tag/1.0.0
