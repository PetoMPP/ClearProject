using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace ClearProject.Git
{
  internal interface IGitService
  {
    Task CreateStash();
    Task ResetWorkingDirectory();
    Task RestoreStash();
  }

  internal class GitService : IGitService
  {
    private record struct ExecOutput(string? Output, string? Error)
    {
      public readonly bool HasError() => !string.IsNullOrWhiteSpace(Error);
    }

    private readonly bool _gitAvailable;
    private readonly IConsoleWriter _consoleWriter;
    private bool _stashAvailable = false;

    public GitService(IConsoleWriter consoleWriter)
    {
      _consoleWriter = consoleWriter;
      _gitAvailable = GetGitVersion()?.GetAwaiter().GetResult() is not null;
    }

    public async Task CreateStash()
    {
      if (!_gitAvailable)
        return;

      int? changes = await GetChanges();
      switch (changes)
      {
        case null or 0:
          _stashAvailable = false;
          _consoleWriter.WriteLine("No changes to preserve!");
          break;
        default:
          if (await StashChanges())
          {
            _stashAvailable = true;
            _consoleWriter.WriteLine($"Stashed {changes} changes!");
            break;
          }
          _stashAvailable = false;
          _consoleWriter.WriteLine("Unable to stash changes!");
          break;
      }
    }

    public async Task RestoreStash()
    {
      if (!_gitAvailable || !_stashAvailable)
        return;

      var stash = Array
        .Find(((await Exec(GitCommands.GetStashes)).Output ?? string.Empty)
          .Split(Environment.NewLine), s => s.Contains(GitCommands.StashName));

      if (stash is null)
      {
        _consoleWriter.WriteLine("Unable to restore stash!");
        return;
      }

      var stashIndex = int.Parse(Regex.Match(stash, @"stash@{(\d)}").Groups[1].Value);
      var result = await Exec(GitCommands.PopStash(stashIndex));
      if (result.HasError())
        _consoleWriter.WriteLine("Unable to restore stash!");

      _consoleWriter.WriteLine("Stash restored!");
    }

    public async Task ResetWorkingDirectory()
    {
      if (!_gitAvailable)
        return;

      await Exec(GitCommands.ResetWorkingDirectory);
    }

    private async Task<ExecOutput> Exec(string cmd)
    {
      var process = new Process
      {
        StartInfo = new ProcessStartInfo
        {
          FileName = "git.exe",
          Verb = "git",
          Arguments = cmd,
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          StandardOutputEncoding = Encoding.UTF8,
          StandardErrorEncoding = Encoding.UTF8,
          CreateNoWindow = true
        },
      };
      process.Start();
      await process.WaitForExitAsync();
      var output = await process.StandardOutput.ReadToEndAsync();
      var error = await process.StandardError.ReadToEndAsync();

      return new(output, error);
    }

    private async Task<bool> StashChanges()
    {
      return string.IsNullOrEmpty((await Exec(GitCommands.StashAll)).Error);
    }

    private async Task<int?> GetChanges()
    {
      var result = await Exec(GitCommands.GetChanges);
      if (!string.IsNullOrWhiteSpace(result.Error))
        return null;

      return result.Output?.Count(c => c == '\n') ?? 0;
    }

    private async Task<string?> GetGitVersion()
    {
      var result = await Exec(GitCommands.GetVersion);
      if (result.HasError())
      {
        _consoleWriter.WriteLine("Git could not be found!");
        return null;
      }

      _consoleWriter.WriteLine($"Using {result.Output}");
      return result.Output;
    }
  }
}
