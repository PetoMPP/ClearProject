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

      _stopwatch.Start();
      console.StartProgressMessage("Working");

      await gitService.CreateStash();
      var result = cleaner.ClearDirectory(dirPath);
      await gitService.ResetWorkingDirectory();
      await gitService.RestoreStash();
      console.StopProgressMessage();

      console.WriteLine($"Deleted {result.Directories.Length} directories and {result.Files} files in {_stopwatch.Elapsed}.");

      console.StartProgressMessage("Press any key to continue");
      Console.ReadKey();
    }
  }
}