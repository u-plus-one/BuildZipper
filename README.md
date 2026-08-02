# Build Zipper

A Unity package for zipping builds automatically, and on Windows, fixing OSX builds by adding missing executable flags in the process.

## Usage

Install the package according to the [installation instructions](#installation). Once installed, every OSX build you create, the post processing script will run, zip your build, and if necessary, fixes said build to be able to run on OSX devices.

### Project settings

This package comes with project specific settings, found at `Edit > Project Settings... > OSX Zip Options`. To start creating zip files when building, select the target platforms you want to create zips for.


| Setting                                         |Description
|-------------------------------------------------|---
| Create Zip[^1]                                  |Whether to create a zip file for the target platform. If unchecked, the build is left untouched.
| Source Build Action[^1]                         |Determies what to do with the original build directory. `Keep` leaves the build directory intact. `Delete` deletes the directory, leaving only the zip file. `Leave Empty Directory` clears out its contents, leaving an empty folder.
| [Zip Creation Method](#zip-creation-method)     |Which method to use for creating the build zip file (Windows Editor only). On macOS and Linux, the zip is created using native zip commands.
| [Zip Compression Level](#zip-compression-level) |The compression level to apply when generating the zip file.
| WSL Process Timeout                             |The time (in seconds) until the wsl process times out.
| Verbose Logging                                 |If checked, prints additional debugging information about the build process.

[^1]: Per-platform setting

#### Zip Creation Method

##### WSL

> [Windows Subsystem for Linux](https://learn.microsoft.com/en-us/windows/wsl/) (WSL) lets developers run a GNU/Linux environment -- including most command-line tools, utilities, and applications -- directly on Windows, unmodified, without the overhead of a traditional virtual machine or dual-boot setup.

> [!WARNING]
> To use this option, WSL has to be installed (preferably an Ubuntu distribution, it's currently the only one we've tested), including the `zip` package on the standard Distribution of WSL on your device.
> 
> - For more information on how to install WSL: https://learn.microsoft.com/en-us/windows/wsl/install/
> - For more information on how to install the `zip` package on Linux (in this case, WSL): https://www.tecmint.com/install-zip-and-unzip-in-linux/#zipubuntu

> [!WARNING]
> To build on external or secondary drives, you have to make sure to *mount* those drives in WSL too: https://learn.microsoft.com/en-us/windows/wsl/wsl2-mount-disk/

##### Zip Manipulation

A Windows-only solution, that zips the build, then edits very specific bytes of the newly created zip, in order to make it compatible with OSX.

#### Zip Compression Level

|Compression Level|[WSL](#wsl)|[Zip Manipulation](#zip-manipulation)
|---|---|---
|None|No compression (`0` in the zip command)|No compression
|Fastest|Fast compression (`1` out of 9 in the zip command)|Fast compression
|Optimal|Best compression (`9` out of 9 in the zip command)|Best compression

## Installation

### Option 1: Package Manager

Open the Package Manager window, click on "Add Package from Git URL ...", then enter the following:
```
https://github.com/u-plus-one/buildzipper.git
```

### Option 2: Manually Editing packages.json

Add the following line to your project's `Packages/manifest.json`:

```json
"com.github.u-plus-one.buildzipper": "https://github.com/u-plus-one/buildzipper.git"
```

### Option 3: Manual Installation (not recommended)

You can also download this repository and extract the `Editor` directory file anywhere inside your project's Assets folder. (The folder _must_ be named 'Editor' for it to work)

## The Problem

Any build for OSX (otherwise known as macOS) built on Windows devices, do in general *not* work. Instead, you get presented with an error, saying `The application "game-name" can't be opened`, with no more information.

The reason why OSX builds built on Windows do not work on OSX, is the fact that Windows does not keep track of unix-specific file attributes (read/write/execute attributes), which are needed for OSX to run an executable. This executable attribute generally can only be added on Unix devices, which both Linux and OSX devices are. The only current solutions for creating an OSX build, are building on either Linux or OSX itself.

## The Solution

This package automatically creates a zip from the mac build and restores the executable bits, which get lost since Windows does not support them.

If you use the [WSL Zip Creation Method](#wsl), it utilises the Windows Subsystem for Linux to zip the build, so OSX knows how to read, unpack, and fix all files, hence adding the executable bit. It's possible to manually add the executable bit to a file on Linux too, with the `chmod` command. If WSL is already installed on the system, this is the recommended method.

The [Zip Manipulation](#zip-manipulation) method, instead, is a Windows-only solution. Instead of relying on a Unix system, it zips the build first, then edits the zip itself and its entries (the files inside), to restore the missing file attributes. To make sure everything works as expected on OSX when unzipping this zip, it changes the zip's Host OS to Unix too, to ensure the required file attributes are applied.

> [!WARNING]
> The zip file should never be unzipped in a Windows environment, since it causes the file attributes to get lost, even when zipped again. Editing the zip's contents with an application (e.g. WinRAR) _should_ be fine.

The zip creation process runs at the very end of the build process, meaning that all changes made during a `IPostProcessBuild` script are included in the build.

## Attribution

[@lajawi](https://github.com/lajawi) - Unix file attributes research, WSL zipper

[@d3tonat0r](https://github.com/d3tonat0r) - Package structure, Manual zip builder
