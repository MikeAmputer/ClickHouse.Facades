namespace ClickHouse.Facades.Testing;

internal class ClickHouseConnectionTracker<TContext>
	where TContext : ClickHouseContext<TContext>
{
	private readonly List<ClickHouseTestResponse> _records = new();

	internal void Add(ClickHouseTestResponse record)
	{
		_records.Add(record);
	}

	public IReadOnlyCollection<ClickHouseTestResponse> GetRecords()
	{
		return _records;
	}
}
