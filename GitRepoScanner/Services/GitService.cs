using System.Diagnostics;
using System.IO;
using GitRepoScanner.Models;

namespace GitRepoScanner.Services;

public class GitService
{
    public IEnumerable<string> FindGitRepos(string rootPath)
    {
        if (!Directory.Exists(rootPath))
            yield break;

        foreach (var gitDir in SafeEnumerateDirectories(rootPath, ".git"))
        {
            var repoPath = Path.GetDirectoryName(gitDir);
            if (repoPath is not null)
                yield return repoPath;
        }
    }

    public async Task<string> GetCurrentBranchAsync(string repoPath)
    {
        var result = await RunGitAsync(repoPath, "rev-parse --abbrev-ref HEAD");
        return string.IsNullOrWhiteSpace(result) ? "(unknown)" : result;
    }

    public async Task<List<FileEntry>> GetUncommittedFilesAsync(string repoPath)
    {
        var output = await RunGitAsync(repoPath, "status --porcelain");
        var entries = new List<FileEntry>();

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Length < 3) continue;
            var status = line[..2].TrimEnd();
            var file = line[3..].Trim();
            if (!string.IsNullOrEmpty(file))
                entries.Add(new FileEntry { Status = status, FilePath = file });
        }

        return entries;
    }

    private static async Task<string> RunGitAsync(string workingDir, string arguments)
    {
        try
        {
            var psi = new ProcessStartInfo("git", arguments)
            {
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi)!;
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            return output.Trim();
        }
        catch
        {
            return "";
        }
    }

    private static IEnumerable<string> SafeEnumerateDirectories(string root, string pattern)
    {
        var queue = new Queue<string>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            string[] subDirs;
            try { subDirs = Directory.GetDirectories(current, pattern); }
            catch { subDirs = []; }

            foreach (var d in subDirs)
                yield return d;

            string[] children;
            try { children = Directory.GetDirectories(current); }
            catch { children = []; }

            foreach (var child in children)
            {
                var name = Path.GetFileName(child);
                if (name == ".git" || name.StartsWith('.'))
                    continue;
                queue.Enqueue(child);
            }
        }
    }
}
