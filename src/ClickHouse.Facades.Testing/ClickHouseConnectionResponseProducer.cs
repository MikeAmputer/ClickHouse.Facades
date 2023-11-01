namespace ClickHouse.Facades.Testing;

internal class ClickHouseConnectionResponseProducer<TContext>
	where TContext : ClickHouseContext<TContext>
{
	private readonly Dictionary<TestQueryType, List<(Predicate<string>, object?)>> _responseDictionary = new();

	internal string? ServerVersion { get; set; } = null;
	internal string? ServerTimezone { get; set; } = null;

	internal void Add(TestQueryType queryType, Predicate<string> sqlPredicate, object? result)
	{
		if (!_responseDictionary.ContainsKey(queryType))
		{
			_responseDictionary.Add(queryType, new List<(Predicate<string>, object?)>());
		}

		_responseDictionary[queryType].Add((sqlPredicate, result));
	}

	internal bool TryGetResponse(TestQueryType queryType, string sql, out object? response)
	{
		if (!_responseDictionary.ContainsKey(queryType))
		{
			response = null;
			return false;
		}

		foreach (var match in _responseDictionary[queryType])
		{
			if (match.Item1(sql))
			{
				response = match.Item2;

				return true;
			}
		}

		response = null;

		return false;
	}
}
