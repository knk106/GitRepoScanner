using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using GitRepoScanner.Models;

namespace GitRepoScanner.ViewModels;

public class RepoViewModel : ViewModelBase
{
    private string _currentBranch = "";
    private bool _isExpanded;
    private bool _isLoading;

    public string Name { get; }
    public string Path { get; }

    public ObservableCollection<FileEntry> UncommittedFiles { get; } = [];

    public string CurrentBranch
    {
        get => _currentBranch;
        set { _currentBranch = value; OnPropertyChanged(); }
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set { _isExpanded = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowCleanBadge));
            OnPropertyChanged(nameof(ShowDirtyBadge));
        }
    }

    public int FileCount => UncommittedFiles.Count;
    public bool IsClean => UncommittedFiles.Count == 0;
    public bool ShowCleanBadge => !IsLoading && IsClean && !string.IsNullOrEmpty(CurrentBranch);
    public bool ShowDirtyBadge => !IsLoading && !IsClean;

    public ICommand ToggleExpandCommand { get; }
    public ICommand OpenFolderCommand { get; }

    public RepoViewModel(GitRepo repo)
    {
        Name = repo.Name;
        Path = repo.Path;

        UncommittedFiles.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(FileCount));
            OnPropertyChanged(nameof(IsClean));
            OnPropertyChanged(nameof(ShowCleanBadge));
            OnPropertyChanged(nameof(ShowDirtyBadge));
        };

        ToggleExpandCommand = new RelayCommand(() => IsExpanded = !IsExpanded);
        OpenFolderCommand   = new RelayCommand(() =>
            Process.Start(new ProcessStartInfo("explorer.exe", repo.Path) { UseShellExecute = true }));
    }

    public void SetFiles(IEnumerable<FileEntry> files)
    {
        UncommittedFiles.Clear();
        foreach (var f in files)
            UncommittedFiles.Add(f);
    }
}
