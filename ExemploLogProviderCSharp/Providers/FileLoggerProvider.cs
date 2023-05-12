using System.Collections.Concurrent;

namespace ExemploLogProviderCSharp.Providers;

[ProviderAlias("FileLogger")]
internal class FileLoggerProvider : ILoggerProvider, ISupportExternalScope
{
	private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();
	private readonly IConfiguration configuration;
	private IExternalScopeProvider? scopeProvider;

	public FileLoggerProvider(IConfiguration configuration)
	{
		this.configuration = configuration;
	}

	public ILogger CreateLogger(string categoryName) =>
		_loggers.GetOrAdd(categoryName, name => new FileLogger(name, configuration, this));

	public void Dispose() => _loggers.Clear();

	public void SetScopeProvider(IExternalScopeProvider scopeProvider) =>
		this.scopeProvider = scopeProvider;


	#region FileLogger

	class FileLogger : ILogger
	{
		private readonly string _name;
		private readonly IConfiguration configuration;
		private readonly FileLoggerProvider provider;

		public FileLogger(string name, IConfiguration configuration, FileLoggerProvider provider)
		{
			_name = name;
			this.configuration = configuration;
			this.provider = provider;
		}

		public IDisposable BeginScope<TState>(TState state) => default;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
				return;

			var linhas = new List<string>
			{
				$"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{eventId.Id,2}: {logLevel,-12}] {_name}"
			};

			if (provider.scopeProvider?.GetScopeLevelGroup() is { } scopeLevelGroup)
				linhas.Add($"Scope: {scopeLevelGroup.GetFullLevelName()}");

			linhas.Add($"\t{formatter(state, exception)}");

			for (var ex = exception; ex != null; ex = ex.InnerException)
				linhas.Add($"\t - {ex.GetType().Name}: {ex.Message}");

			linhas.Add(string.Empty);

			AppendToLogFile(linhas);
		}

		private static object _lockWriter = new object();
		private void AppendToLogFile(IEnumerable<string> lines)
		{
			var storagePath = configuration["Logging:FileLogger:StoragePath"];
			var fileNamePrefix = configuration["Logging:FileLogger:FileNamePrefix"];
			var fileSizeLimit = configuration["Logging:FileLogger:FileSizeLimit"];
			string logFileName = $"{fileNamePrefix}-{DateTime.Now:yyyy-MM-dd}.log";

			if (string.IsNullOrWhiteSpace(storagePath))
				return;

			lock (_lockWriter)
			{
				var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, storagePath);

				if (!Directory.Exists(logDir))
					Directory.CreateDirectory(logDir);

				string logFilePath = Path.Combine(logDir, logFileName);

				RenameFileIfExceedSizeLimit(logFilePath, fileSizeLimit);

				using var writer = File.AppendText(logFilePath);

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

		private static void RenameFileIfExceedSizeLimit(string filePath, string sizeLimit)
		{
			var fileInfo = new FileInfo(filePath);
			var fileSizeLimitInBytes = ParseByteSize(sizeLimit) ?? 20 * 1024 * 1024;

			if (!fileInfo.Exists || fileInfo.Length < fileSizeLimitInBytes)
				return;

			var n = 1;
			do
			{
				var newName = $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}-({n++}){fileInfo.Extension}";
				var newFilePath = Path.Combine(fileInfo.DirectoryName, newName);

				if (!File.Exists(newFilePath))
				{
					File.Move(filePath, newFilePath);
					break;
				}
			} while (true);
		}

		private static int? ParseByteSize(string value)
		{
			if (value == null)
				return 0;

			value = value.TrimStart().TrimEnd(' ', 'b', 'B');
			var suffix = string.Empty;

			if (value.Length > 0 && char.IsLetter(value, value.Length - 1))
			{
				suffix = value.Substring(value.Length - 1, 1);
				value = value.Substring(0, value.Length - suffix.Length);
			}

			if (!int.TryParse(value, out var sizeInBytes))
				return null;

			if (suffix == "K")
				sizeInBytes = sizeInBytes * 1024;
			else if (suffix == "M")
				sizeInBytes = sizeInBytes * 1024 * 1024;
			else if (suffix == "G")
				sizeInBytes = sizeInBytes * 1024 * 1024 * 1024;
			else if (suffix == "T")
				sizeInBytes = sizeInBytes * 1024 * 1024 * 1024 * 1024;
			else if (suffix != string.Empty)
				return null;

			return sizeInBytes;
		}
	}

	#endregion
}