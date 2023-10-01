namespace ClearProject
{
  internal interface ICleaner
  {
    CleanerResult ClearDirectory(string dirPath);
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
