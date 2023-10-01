namespace ClearProject
{
  public record struct CleanerResult(string[] Directories, int Files)
  {
    private static readonly CleanerResult _empty = new(Array.Empty<string>(), 0);
    public static readonly CleanerResult Empty = _empty;

    public static CleanerResult operator +(CleanerResult a, CleanerResult b)
    {
      var result = new CleanerResult(new string[a.Directories.Length + b.Directories.Length], a.Files + b.Files);
      Array.Copy(a.Directories, 0, result.Directories, 0, a.Directories.Length);
      Array.Copy(b.Directories, 0, result.Directories, a.Directories.Length, b.Directories.Length);
      return result;
    }
  }
}
