using System.Collections.Concurrent;

namespace ExemploLogProviderCSharp.Providers;

[ProviderAlias("FileLogger")]
internal class FileLoggerProvider : ILoggerProvider
{
	private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();
	private readonly IConfiguration configuration;

	public FileLoggerProvider(IConfiguration configuration)
	{
		this.configuration = configuration;
	}

	public ILogger CreateLogger(string categoryName) =>
		_loggers.GetOrAdd(categoryName, name => new FileLogger(name, configuration));

	public void Dispose() => _loggers.Clear();


	#region FileLogger

	class FileLogger : ILogger
	{
		private readonly string _name;
		private readonly IConfiguration configuration;

		public FileLogger(string name, IConfiguration configuration)
		{
			this._name = name;
			this.configuration = configuration;
		}

		public IDisposable BeginScope<TState>(TState state) => default;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
				return;

			var linhas = new List<string>()
				{
					$"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{eventId.Id,2}: {logLevel,-12}] {_name}",
					$"\t{formatter(state, exception)}",
				};

			if (exception is { } ex)
			{
				while (ex != null)
				{
					linhas.Add($"\t - {ex.GetType().Name}: {ex.Message}");
					ex = ex.InnerException;
				}
			}

			AppendToLogFile(linhas);
		}

		private static object _lockWriter = new object();
		private void AppendToLogFile(IEnumerable<string> lines)
		{
			var logSubDir = configuration["LogStoragePaths"];

			if (string.IsNullOrWhiteSpace(logSubDir))
				return;

			lock (_lockWriter)
			{
				var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logSubDir);

				if (!Directory.Exists(logDir))
					Directory.CreateDirectory(logDir);

				using var writer = File.AppendText(Path.Combine(logDir, $"registro-log-{DateTime.Now:yyyy-MM-dd}.log"));

				try
				{
					foreach (var line in lines)
						writer.WriteLine(line);
				}
				finally
				{
					writer.Close();
				}
			}
		}

	}

	#endregion
}