using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryXmlProcessor.Services.Logging;

public enum LogLevel
{
    Filtering,
    Transformation,
    Saving
}

public sealed class EventLogger
{
    private static EventLogger? _instance;
    private static readonly object _lock = new();
    private readonly string _logFilePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private EventLogger()
    {
        var logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        if (!Directory.Exists(logsDirectory))
        {
            Directory.CreateDirectory(logsDirectory);
        }

        _logFilePath = Path.Combine(logsDirectory, $"library_log_{DateTime.Now:yyyyMMdd}.txt");
    }

    public static EventLogger Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new EventLogger();
                    }
                }
            }
            return _instance;
        }
    }

    public void Log(LogLevel level, string message)
    {
        _semaphore.Wait();
        try
        {
            var timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            var levelName = GetLevelName(level);
            var logEntry = $"{timestamp} {levelName}. {message}";

            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task LogAsync(LogLevel level, string message)
    {
        await _semaphore.WaitAsync();
        try
        {
            var timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            var levelName = GetLevelName(level);
            var logEntry = $"{timestamp} {levelName}. {message}";

            await File.AppendAllTextAsync(_logFilePath, logEntry + Environment.NewLine);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private string GetLevelName(LogLevel level)
    {
        return level switch
        {
            LogLevel.Filtering => "Фільтрація",
            LogLevel.Transformation => "Трансформація",
            LogLevel.Saving => "Збереження",
            _ => "Невідомо"
        };
    }

    public string GetLogFilePath() => _logFilePath;
}
