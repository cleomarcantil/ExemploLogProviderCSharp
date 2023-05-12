namespace Microsoft.Extensions.Logging;

public static class LoggingUtilsExtensions
{
	public const string SCOPED_LOG_ID = "__ScopedLogId";
	public const string SCOPED_LOG_NAME = "__ScopedLogName";

	public static IDisposable? BeginScopeLevelGroup(this ILogger logger, string? name = null)
	{
		string id = Guid.NewGuid().ToString("N").ToUpper();

		var scopeValues = new List<KeyValuePair<string, object>>
		{
			new(SCOPED_LOG_ID, id),
		};

		if (name is not null)
			scopeValues.Add(new(SCOPED_LOG_NAME, name));

		return logger.BeginScope(scopeValues);
	}


	internal record ScopeLevelGroup(string Id, string? Name, ScopeLevelGroup? Parent)
	{
		public override string ToString() =>
			(Name is not null) ? $"[{Id},{Name}]" : $"[{Id}]";
		
		public string GetFullLevelName() =>
			(Parent is not null) ? $"{Parent.GetFullLevelName()}:{ToString()}" : ToString();
	}

	internal static ScopeLevelGroup? GetScopeLevelGroup(this IExternalScopeProvider scopeProvider)
	{
		if (scopeProvider == null)
			return null;

		ScopeLevelGroup? group = null;

		scopeProvider.ForEachScope<object?>((scopeProps, _) =>
		{
			if (scopeProps as IEnumerable<KeyValuePair<string, object>> is { } valueProps)
			{
				var id = valueProps.FirstOrDefault(x => x.Key == SCOPED_LOG_ID).Value as string;
				var name = valueProps.FirstOrDefault(x => x.Key == SCOPED_LOG_NAME).Value as string;

				if (id is not null)
					group = new ScopeLevelGroup(id, name, group);
			}

		}, null);

		return group;
	}

}