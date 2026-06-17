using System.Collections.ObjectModel;
using System.Windows.Input;
using GitRepoScanner.Models;
using GitRepoScanner.Services;

namespace GitRepoScanner.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly GitService _git = new();
    private readonly SettingsService _settings = new();

    private string _rootPath = "";
    private string _statusText = "Ready — click Scan to discover repositories.";
    private bool _isBusy;
    private string _lastScanned = "";
    private string _lastRefreshed = "";

    public ObservableCollection<RepoViewModel> Repos { get; } = [];

    public string RootPath
    {
        get => _rootPath;
        set
        {
            if (_rootPath == value) return;
            _rootPath = value;
            OnPropertyChanged();
            _settings.SaveSettings(new AppSettings { RootPath = value });
        }
    }

    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoRepos)); }
    }

    public string LastScanned
    {
        get => _lastScanned;
        set { _lastScanned = value; OnPropertyChanged(); }
    }

    public string LastRefreshed
    {
        get => _lastRefreshed;
        set { _lastRefreshed = value; OnPropertyChanged(); }
    }

    public bool HasNoRepos => !IsBusy && Repos.Count == 0;

    public ICommand ScanCommand    { get; }
    public ICommand RefreshCommand { get; }
    public ICommand BrowseCommand  { get; }

    public MainViewModel()
    {
        var saved = _settings.LoadSettings();
        _rootPath = saved.RootPath;

        ScanCommand    = new RelayCommand(ScanAsync,    () => !IsBusy);
        RefreshCommand = new RelayCommand(RefreshAsync, () => !IsBusy && Repos.Count > 0);
        BrowseCommand  = new RelayCommand(Browse,       () => !IsBusy);

        var cachedRepos = _settings.LoadRepos();
        foreach (var r in cachedRepos)
            Repos.Add(new RepoViewModel(r));

        if (Repos.Count > 0)
            StatusText = $"{Repos.Count} repos loaded from cache…";

        OnPropertyChanged(nameof(HasNoRepos));
    }

    public async Task InitializeAsync()
    {
        if (Repos.Count > 0)
            await RefreshAsync();
    }

    private async Task ScanAsync()
    {
        IsBusy = true;
        StatusText = "Scanning for git repositories…";
        Repos.Clear();
        OnPropertyChanged(nameof(HasNoRepos));

        try
        {
            var found = await Task.Run(() =>
                _git.FindGitRepos(RootPath)
                    .Select(p => new GitRepo { Name = System.IO.Path.GetFileName(p), Path = p })
                    .OrderBy(r => r.Name)
                    .ToList());

            _settings.SaveRepos(found);

            foreach (var r in found)
                Repos.Add(new RepoViewModel(r));

            LastScanned = DateTime.Now.ToString("HH:mm:ss");
            StatusText = $"{Repos.Count} repositories found.";
            OnPropertyChanged(nameof(HasNoRepos));

            if (Repos.Count > 0)
                await RefreshAsync();
        }
        catch (Exception ex)
        {
            StatusText = $"Scan error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshAsync()
    {
        IsBusy = true;
        StatusText = "Refreshing branch and status…";

        try
        {
            var tasks = Repos.Select(async repo =>
            {
                repo.IsLoading = true;
                try
                {
                    repo.CurrentBranch = await _git.GetCurrentBranchAsync(repo.Path);
                    var files = await _git.GetUncommittedFilesAsync(repo.Path);
                    repo.SetFiles(files);
                }
                catch
                {
                    repo.CurrentBranch = "(error)";
                }
                finally
                {
                    repo.IsLoading = false;
                }
            }).ToList();

            await Task.WhenAll(tasks);

            LastRefreshed = DateTime.Now.ToString("HH:mm:ss");
            var dirty = Repos.Count(r => !r.IsClean);
            StatusText = dirty > 0
                ? $"{Repos.Count} repos  —  {dirty} with uncommitted changes"
                : $"{Repos.Count} repos  —  all clean";
        }
        catch (Exception ex)
        {
            StatusText = $"Refresh error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Browse()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select root folder to scan for git repositories",
            InitialDirectory = RootPath
        };
        if (dialog.ShowDialog() == true)
            RootPath = dialog.FolderName;
    }
}
