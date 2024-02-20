using ClearProject.Git;
using System.Diagnostics;

namespace ClearProject
{
  internal static class Program
  {
    private static readonly Stopwatch _stopwatch = new();

    static async Task Main(string[] args)
    {
      var console = new ConsoleWriter();
      var gitService = new GitService(console);
      var cleaner = new Cleaner();
      var dirPath = args.FirstOrDefault() ?? console.AskForDirectoryPath();
      var clearNuGetCache = args.Length > 1 && (args[1] == "--nuget" || args[1] == "-n");

      _stopwatch.Start();
      console.StartProgressMessage("Working");

      await gitService.CreateStash();
      var result = cleaner.ClearDirectory(dirPath);
      if (clearNuGetCache)
      {
        result += cleaner.ClearNuGetCache(dirPath);
      }
      await gitService.ResetWorkingDirectory();
      await gitService.RestoreStash();
      console.StopProgressMessage();

      console.WriteLine($"Deleted {result.Directories.Length} directories and {result.Files} files in {_stopwatch.Elapsed}.");

      console.StartProgressMessage("Press any key to continue");
      Console.ReadKey();
    }
  }
}