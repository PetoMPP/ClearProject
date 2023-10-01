namespace ClearProject.Git
{
  internal static class GitCommands
  {
    internal static readonly string StashName = $"clear-project{Random.Shared.Next(0xFFFFF):X5}";

    internal static readonly string GetVersion = "--version";
    internal static readonly string GetChanges = "status -s";
    internal static readonly string GetStashes = "stash list";
    internal static readonly string StashAll = $"stash -u -m {StashName}";
    internal static readonly string ResetWorkingDirectory = "reset --hard";

    internal static string PopStash(int stashIndex) => $"stash pop --index {stashIndex}";
  }
}
