namespace ClearProject
{
  internal interface IConsoleWriter
  {
    string AskForDirectoryPath();
    void StartProgressMessage(string message);
    void StopProgressMessage();
    void Write(string? value);
    void WriteLine(string? value);
  }

  internal class ConsoleWriter : IConsoleWriter
  {
    private static readonly TimeSpan _defaultTick = TimeSpan.FromMilliseconds(500);
    private readonly PeriodicTimer _timer;
    private CancellationTokenSource _cts;
    private CancellationToken _token;
    private readonly SemaphoreSlim _semaphore = new(1);
    private bool _inProgress;
    private string? _message;

    public ConsoleWriter(TimeSpan? tick = null)
    {
      _timer = new(tick ?? _defaultTick);
      _cts = new();
      _token = _cts.Token;
    }

    public string AskForDirectoryPath()
    {
      Write("Please enter cleared .NET project path: ");
      return Console.ReadLine() ?? string.Empty;
    }

    public void Write(string? value)
    {
      WriteToConsole(value);
    }

    public void WriteLine(string? value)
    {
      Write(value + Environment.NewLine);
    }

    public async void StartProgressMessage(string message)
    {
      try
      {
        if (_cts.IsCancellationRequested)
          ResetCancellationState();

        message = $"\r{message}";
        _message = message;
        var i = 0;
        Console.CursorVisible = false;
        _inProgress = true;
        while (await _timer.WaitForNextTickAsync(_token))
        {
          i = i > 3 ? 0 : i;
          _message = message + new string('.', i) + new string(' ', 3 - i);
          Write(null);
          i++;
        }
      }
      catch
      {
      }
      finally
      {
        var clearMessage = '\r' + new string(' ', _message?.Length ?? 0) + '\r';
        _inProgress = false;
        _message = null;
        Write(clearMessage);
      }
    }

    public void StopProgressMessage() => _cts.Cancel();

    private void ResetCancellationState()
    {
      _cts = new();
      _token = _cts.Token;
    }

    private void WriteToConsole(string? value)
    {
      _semaphore.Wait();
      Console.Write(_inProgress
        ? value is not null
          ? '\r' + value + _message
          : _message
        : value);
      _semaphore.Release();
    }
  }
}
