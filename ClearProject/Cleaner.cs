namespace ClearProject
{
  internal interface ICleaner
  {
    CleanerResult ClearDirectory(string dirPath);
    CleanerResult ClearNuGetCache(string dirPath);
  }

  internal class Cleaner : ICleaner
  {
    private readonly List<string> _deleted = new();
    private int _deletedCount;

    public CleanerResult ClearDirectory(string dirPath)
    {
      DeleteBinObj(dirPath);
      var result = new CleanerResult(_deleted.ToArray(), _deletedCount);
      _deleted.Clear();
      _deletedCount = 0;
      return result;
    }

    public CleanerResult ClearNuGetCache(string dirPath)
    {
      if (!Directory.Exists(dirPath))
      {
        Console.WriteLine($"Directory {dirPath} doesn't exist!");
        return CleanerResult.Empty;
      }

      string[] nugetCaches = [
        Path.Combine(dirPath, "packages"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NuGet", "Cache"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NuGet", "v3-cache"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NuGet", "plugins-cache"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft SDKs", "NuGetPackages")
      ];
      var deletedCount = 0;
      var deleted = new List<string>();
      foreach (string nugetCache in nugetCaches)
      {
        if (!Directory.Exists(nugetCache))
        {
          Console.WriteLine($"Directory {nugetCache} doesn't exist!");
          continue;
        }

        try
        {
          deletedCount += Directory.GetFiles(nugetCache, "*.*", SearchOption.AllDirectories).Length;
          deleted.Add(nugetCache);
          Directory.Delete(nugetCache, true);
        }
        catch (Exception error)
        {
          Console.WriteLine(error.Message);
        }
      }

      return new CleanerResult(deleted.ToArray(), deletedCount);
    }

    private void DeleteBinObj(string dirPath)
    {
      if (!Directory.Exists(dirPath))
      {
        Console.WriteLine($"Directory {dirPath} doesn't exist!");
        return;
      }

      foreach (var dir in Directory.GetDirectories(dirPath))
      {
        if (!dir.EndsWith("obj") && !dir.EndsWith("bin"))
        {
          DeleteBinObj(dir);
          continue;
        }

        try
        {
          _deletedCount += Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).Length;
          _deleted.Add(dir);
          Directory.Delete(dir, true);
        }
        catch (Exception error)
        {
          Console.WriteLine(error.Message);
        }
      }
    }
  }
}
