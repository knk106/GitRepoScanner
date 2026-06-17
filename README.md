# Git Repo Scanner

A lightweight WPF desktop utility that recursively scans a root folder for git repositories and shows each repo's current branch and uncommitted files at a glance.

## Features

- Recursively discovers all git repositories under a configurable root path
- Displays current branch and uncommitted file count per repo
- Expands each repo row to list all modified, added, deleted, and untracked files
- Color-coded file status (modified = amber, added = green, deleted = red, untracked = purple)
- Auto-refreshes branch and status on every launch (using the last saved repo list)
- Opens any repo folder directly in Windows Explorer
- Saves discovered repos to `repos.json` and root path setting to `settings.json` next to the exe

## Usage

### Scan
Click **Scan** to recursively search the root path for git repositories. Results are saved to `repos.json` so the list persists across restarts.

### Refresh
Click **Refresh** to re-run `git branch` and `git status` on all known repos without rescanning the disk. This also runs automatically on every launch if a saved repo list exists.

### Browse
Click **Browse** to change the root path. The new path is saved immediately to `settings.json`.

### Open
Click the **Open** button on any repo row to open that folder in Windows Explorer.

## Requirements

### Running the portable exe
- Windows 10 or later
- No additional dependencies — the runtime is bundled inside the exe

### Building from source
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Git installed and available on `PATH`

## Build

```bash
dotnet build GitRepoScanner/GitRepoScanner.csproj
```

## Publish as single portable exe

```bash
dotnet publish GitRepoScanner/GitRepoScanner.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Output: `GitRepoScanner/bin/Release/net8.0-windows/win-x64/publish/GitRepoScanner.exe`

## Project structure

```
GitRepoScanner/
├── Models/
│   ├── AppSettings.cs       root path setting
│   ├── GitRepo.cs           repo name + path
│   └── FileEntry.cs         git status code + file path
├── Services/
│   ├── GitService.cs        recursive repo discovery, git commands
│   └── SettingsService.cs   settings.json and repos.json persistence
├── ViewModels/
│   ├── MainViewModel.cs     scan / refresh / browse logic
│   └── RepoViewModel.cs     per-repo state and commands
├── Converters/              WPF value converters
├── MainWindow.xaml          UI layout
└── GitRepoScanner.csproj    .NET 8 WPF, no external NuGet dependencies
```

## License

MIT
