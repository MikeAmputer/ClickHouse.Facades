namespace ClickHouse.Facades.Testing;

internal class ClickHouseConnectionResponseProducer<TContext>
	where TContext : ClickHouseContext<TContext>
{
	private readonly Dictionary<TestQueryType, Stack<(Predicate<string>, Func<object?>)>> _responseDictionary = new();

	private Func<string?>? _serverVersionProvider;
	private Func<string?>? _serverTimezoneProvider;

	internal void SetServerVersionProvider(Func<string?> serverVersionProvider)
	{
		_serverVersionProvider = serverVersionProvider;
	}

	internal void SetServerTimezoneProvider(Func<string?> serverTimezoneProvider)
	{
		_serverTimezoneProvider = serverTimezoneProvider;
	}

	internal string? ServerVersion => _serverVersionProvider?.Invoke();
	internal string? ServerTimezone => _serverTimezoneProvider?.Invoke();

	internal void Add(TestQueryType queryType, Predicate<string> sqlPredicate, Func<object?> result)
	{
		if (!_responseDictionary.ContainsKey(queryType))
		{
			_responseDictionary.Add(queryType, new Stack<(Predicate<string>, Func<object?>)>());
		}

		_responseDictionary[queryType].Push((sqlPredicate, result));
	}

	internal bool TryGetResponse(TestQueryType queryType, string sql, out object? response)
	{
		if (!_responseDictionary.TryGetValue(queryType, out var value))
		{
			response = null;
			return false;
		}

		foreach (var match in value)
		{
			if (match.Item1(sql))
			{
				response = match.Item2();

				return true;
			}
		}

		response = null;

		return false;
	}
}
